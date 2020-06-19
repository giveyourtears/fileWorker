using Inex.Umk.IO;
using Inex.Umk.Messages;
using Inex.Umk.Models;
using Inex.Umk.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Inex.Umk.Controllers
{
  [Route("api/[controller]/[action]")]
  [ApiController]
  public class UploadDownloadController : ControllerBase
  {
    private FileDbService _fileService;
    internal ILogger<UploadDownloadController> _log;
    IOptions<AppSettings> _settings;
    public UploadDownloadController(
      FileDbService fileService,
      ILogger<UploadDownloadController> logger,
      IOptions<AppSettings> settings
      )
    {
      _log = logger;
      _fileService = fileService;
      _settings = settings;
    }

    private async Task<ProcessResponse> SendToPdfService(IFormFile file)
    {
      _log.LogDebug($"Send file={file} to convert");
      HttpClientHandler clientHandler = new HttpClientHandler();
      clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
      using (HttpClient client = new HttpClient(clientHandler))
      {
        ProcessResponse responsePdf;
        byte[] data;
        using (BinaryReader br = new BinaryReader(file.OpenReadStream()))
        {
          data = br.ReadBytes((int)file.OpenReadStream().Length);
        }
        ByteArrayContent bytes = new ByteArrayContent(data);
        MultipartFormDataContent multiContent = new MultipartFormDataContent();
        multiContent.Add(bytes, "file", file.FileName);
        string baseUri = Utils.ReplaceEnvVars(_settings.Value.ConvUri);
        HttpResponseMessage result = await client.PostAsync(baseUri.TrimEnd('/') + "/api/filepdf/process", multiContent);
        _log.LogDebug("UploadDownloadController.SendToPdfService() OK");
        return responsePdf = JsonConvert.DeserializeObject<ProcessResponse>(await result.Content.ReadAsStringAsync());
      }
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
      try
      {
        _log.LogDebug($"UploadFile file={file}");
        string fileExtension = Path.GetExtension(file.FileName).Trim('.');
        if (file == null || file.Length == 0)
          throw new Exception("Файл не может быть пуст");
        if (CheckFileExtension.GetFileExtension(fileExtension) == FileExtension.Extensions.Unknown)
          throw new Exception("Неподдерживаемый тип файла");
        string currentSid = Utils.GetSid(HttpContext);
        Document document = new Document();
        PdfText text = null;
        ProcessResponse result;
        string dirPdfPath = Folder.GetAllPath(Path.Combine(Umk.IO.Folder.UploadsFolder, "pdf"));
        string filePdfPath = Path.Combine(dirPdfPath, $"{Path.GetFileNameWithoutExtension(file.FileName)}.pdf");
        if (CheckFileExtension.GetFileExtension(fileExtension) == FileExtension.Extensions.Pdf)
        {
          result = await SendToPdfService(file);
          string documentId = Guid.NewGuid().ToString();
          foreach (var infoPdf in result.Values)
          {
            document = new Document { doc_id = documentId, parent_id = null, path = filePdfPath.Substring(filePdfPath.IndexOf("\\uploads") + 1), type = Convert.ToByte(CheckFileExtension.GetFileExtension("pdf")), upload_time_stamp = DateTime.Now, short_name = Path.GetFileNameWithoutExtension(filePdfPath), user_sid = currentSid, picture = "data:image/jpg;base64," + Convert.ToBase64String(infoPdf.Image) };
            _fileService.Add(document, file, filePdfPath, (f, ff) =>
            {
              using (var pdfFileStream = new FileStream(filePdfPath, FileMode.Create))
              {
                file.CopyTo(pdfFileStream);
              }
            });
            foreach (var pdfText in infoPdf.TextValues)
            {
              text = new PdfText { parent_id = documentId, page_number = pdfText.Key, page_text = pdfText.Value, short_name = document.short_name };
            }
          }
        }
        else
        {
          string dir = Folder.GetAllPath(Path.Combine(Umk.IO.Folder.UploadsFolder, fileExtension));
          string filePath = Path.Combine(dir, file.FileName);
          string documentId = Guid.NewGuid().ToString();
          document = new Models.Document { doc_id = documentId, parent_id = null, path = filePath.Substring(filePath.IndexOf("\\uploads") + 1), type = Convert.ToByte(CheckFileExtension.GetFileExtension(fileExtension)), upload_time_stamp = DateTime.Now, short_name = Path.GetFileNameWithoutExtension(filePath), user_sid = currentSid, picture = null };
          _fileService.Add(document, file, filePath, (f, ff) =>
          {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
              file.CopyTo(fileStream);
            }
          });
          result = await SendToPdfService(file);
          foreach (var infoPdf in result.Values)
          {
            document = new Models.Document { doc_id = Guid.NewGuid().ToString(), parent_id = documentId, path = filePdfPath.Substring(filePdfPath.IndexOf("\\uploads") + 1), type = Convert.ToByte(CheckFileExtension.GetFileExtension("pdf")), upload_time_stamp = DateTime.Now, short_name = Path.GetFileNameWithoutExtension(filePdfPath), user_sid = currentSid, picture = "data:image/jpg;base64," + Convert.ToBase64String(infoPdf.Image) };
            _fileService.Add(document, file, filePdfPath, (f, ff) =>
            {
              using (var pdfFileStream = new FileStream(filePdfPath, FileMode.Create))
              using (Stream streamPdf = new MemoryStream(infoPdf.Pdf))
              {
                streamPdf.CopyTo(pdfFileStream);
              }
            });
            foreach (var pdfText in infoPdf.TextValues)
            {
              text = new PdfText { parent_id = documentId, page_number = pdfText.Key, page_text = pdfText.Value, short_name = document.short_name };
            }
          }
        }
        _log.LogDebug("UploadDownloadController.Upload() OK");
        return Ok(document);
      }
      catch (Exception e)
      {
        _log.LogError(e.Message);
        return BadRequest(e.Message);
      }
    }

    [HttpGet]
    public IActionResult Files()
    {
      _log.LogDebug($"Return files Files() ");
      try
      {
        string currentSid = Utils.GetSid(HttpContext);
        List<DocumentViewModel> uploads = _fileService.FilterFilesByGuid(currentSid).ToList();
        _log.LogDebug("UploadDownloadController.Files() OK");
        return Ok(uploads);
      }
      catch (Exception e)
      {
        _log.LogError(e.Message);
        return BadRequest(e.Message);
      }
    }

    private string GetContentType(string path)
    {
      var provider = new FileExtensionContentTypeProvider();
      string contentType;
      if (!provider.TryGetContentType(path, out contentType))
      {
        contentType = "application/octet-stream"; // двоичный файл без указания формата
      }
      return contentType;
    }
  }
}
