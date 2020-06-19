using Inex.Umk.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using Viten.QueryBuilder.Data.AnyDb;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Http;
using Viten.QueryBuilder;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Inex.Umk.Services
{
  public class FileDbService
  {
    AnyDbFactory _factory;
    ILogger<FileDbService> _logger;

    public FileDbService(AnyDbFactory factory, ILogger<FileDbService> logger)
    {
      _factory = factory;
      _logger = logger;
    }

    public void Add(Document document, IFormFile file, string path, Action<IFormFile, string> copyToDisc)
    {
      _logger.LogDebug($"Add to database file={file}");
      using (AnyDbConnection connection = _factory.OpenConnection())
      using (DbTransaction transaction = connection.BeginTransaction())
      {
        try
        {
          connection.Insert(document, transaction: transaction);
          copyToDisc(file, path);
          transaction.Commit();
        }
        catch
        {
          transaction.Rollback();
          throw;
        }
      }
      _logger.LogDebug($"FileDbService.Add .... OK");
    }

    public IEnumerable<DocumentViewModel> FilterFilesByGuid(string currentSid)
    {
      _logger.LogDebug($"Return files from database by current sid");
      Select select = Qb.Select(nameof(Document.path), nameof(Document.upload_time_stamp), nameof(Document.short_name), nameof(Document.picture), nameof(Document.doc_id))
        .From(Tables.Document)
        .Where(
          Cond.Equal(nameof(Document.user_sid), currentSid),
          Cond.Equal(nameof(Document.type), Convert.ToByte(IO.FileExtension.Extensions.Pdf))
        );
      IEnumerable<DocumentViewModel> documents;
      try
      {
        using (AnyDbConnection connection = _factory.OpenConnection())
        {
          return documents = connection.Query<DocumentViewModel>(select);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
      }
      _logger.LogDebug($"FileDbService.FilterFilesByGuid....OK");
      return null;
    }
  }
}
