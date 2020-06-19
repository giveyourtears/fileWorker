using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inex.Umk
{
  public class AppSettings
  {
    public bool IsCommunityEdition { get; set; }
    public string AuthUri { get; set; }
    public string ConvUri { get; set; }
    public string AuthAdminUri { get; set; }
    /// <summary>
    /// Секрет клиентского приложения (Angular)
    /// </summary>
    //public string ClientSecret { get; set; }
    public string[] MigrationAssemblies { get; set; }
    /// <summary>
    /// Класс реализации FTS
    /// </summary>
    public string FtsImpl { get; set; }
  }
}
