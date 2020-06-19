using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inex.Umk.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Viten.QueryBuilder.Data.AnyDb;

namespace Inex.Umk
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services
        .AddMvc()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
        .AddControllersAsServices();
      services.Configure<AppSettings>(Configuration);
      services.AddCors();
      services.AddSingleton<AnyDbFactory>(new AnyDbFactory(new Db.AnyDbSettings(Configuration), new Db.AnyDbConsoleAnnouncer(Configuration)));
      services.AddTransient<AppUserGroupService>();
      services.AddTransient<IdentityService>();
      services.AddSingleton<AuditService>();
      services.AddTransient<FileDbService>();
      services.AddTransient<FtsService>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseHsts();
      }
      app.UseStaticFiles();
      app.UseCors(b =>
          b.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader());
      app.UseHttpsRedirection();
      app.UseMvc();
    }
  }
}
