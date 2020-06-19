using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Viten.QueryBuilder;
using Viten.QueryBuilder.Data.AnyDb;

namespace Inex.Umk
{
  public class AnyDbSettings : IAnyDbSetting
  {
    public AnyDbSettings(IConfiguration config)
    {
      DatabaseProvider = DatabaseProvider.Parse<DatabaseProvider>(config.GetValue<string>("DatabaseProvider", "PostgreSql"));
      ConnectionString = config.GetValue<string>("ConnectionString");
      CommandTimeout = config.GetValue<int>("CommandTimeout", 30);
    }
    DatabaseProvider _databaseProvider;
    public DatabaseProvider DatabaseProvider { get => _databaseProvider; set => _databaseProvider = value; }

    string _connectionString;
    public string ConnectionString { get => _connectionString; set => _connectionString = value; }

    int _commandTimeout;
    public int CommandTimeout { get => _commandTimeout; set => _commandTimeout = value; }
  }
}
