using System.IO;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inex.Umk
{
  public class Program
  {
    public static void Main(string[] args)
    {
      Dapper.AnyDbConnectionInitialiser.Initialise();
      IWebHost host = CreateWebHostBuilder(args).Build();
      IConfiguration config = (IConfiguration)host.Services.GetService(typeof(IConfiguration));
      Inex.Umk.Migrate.Runner.Run(config);
      host.Run();
    }

    public static IWebHostBuilder CreateWebHostBuilder(string[] args)
    {
      IWebHostBuilder hostBuilder = WebHost.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
          config.SetBasePath(IO.Folder.CurrentFolder);
          IHostingEnvironment env = context.HostingEnvironment;
          config.AddJsonFile(Path.Combine(IO.Folder.ConfigFolder, "appsettings.json"), optional: true, reloadOnChange: true)
                            .AddJsonFile(Path.Combine(IO.Folder.ConfigFolder, $"appsettings.{env.EnvironmentName}.json"),
                                optional: true, reloadOnChange: true);
        })
        .ConfigureLogging((logingContext, b) =>
        {
          IConfigurationSection section = logingContext.Configuration.GetSection("Logging");
          b.AddConfiguration(section);
          b.ClearProviders();
          b.AddConsole();
          b.AddFluentMigratorConsole();
          b.AddFile(section);
        })
        .UseStartup<Startup>();
      return hostBuilder;
    }
  }
}
