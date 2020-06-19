using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Inex.Umk.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class AboutController : ControllerBase
  {
    IOptions<AppSettings> _settings;
    ILogger<AboutController> _logger;
    public AboutController(
      IOptions<AppSettings> settings,
      ILogger<AboutController> logger
      )
    {
      _settings = settings;
      _logger = logger;
    }
    // GET api/about
    [HttpGet]
    public ActionResult<AboutInfo> Get()
    {
      _logger.LogDebug("AboutController.Get()");
      AboutInfo ai = new AboutInfo()
      {
        Settings = _settings.Value,
      };
      Assembly assm = Assembly.GetExecutingAssembly();
      ai.Copyright = assm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
      ai.Trademark = assm.GetCustomAttribute<AssemblyTrademarkAttribute>().Trademark;
      ai.InformationalVersion = assm.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
      ai.Envs.Add("DEV_PG_HOST", Environment.GetEnvironmentVariable("DEV_PG_HOST"));
      ai.Envs.Add("ASPNETCORE_ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
      _logger.LogDebug("AboutController.Get()..OK");
      return Ok(ai);
    }
  }
  /// <summary>
  /// Информация о продукте
  /// </summary>
  public class AboutInfo
  {
    public string Copyright { get; set; }
    public string Trademark { get; set; }
    public string InformationalVersion { get; set; }
    /// <summary>
    /// Текущие настройки
    /// </summary>
    public AppSettings Settings { get; set; }
    /// <summary>
    /// Значения специальных переменных окружения
    /// </summary>
    public Dictionary<string, string> Envs = new Dictionary<string, string>();
  }
}
