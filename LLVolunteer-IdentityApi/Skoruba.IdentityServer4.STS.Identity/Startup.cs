using System.Collections.Generic;
using Extensions.Convert.ConvertHelper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Skoruba.IdentityServer4.Admin.EntityFramework.DbContexts;
using Skoruba.IdentityServer4.Admin.EntityFramework.Identity.Entities.Identity;
using Skoruba.IdentityServer4.STS.Identity.Helpers;

namespace Skoruba.IdentityServer4.STS.Identity
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        private readonly ILogger _logger;

        public Startup(IHostingEnvironment environment ,ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            //if (environment.IsDevelopment())
            //{
            //    builder.AddUserSecrets<Startup>();
            //}

            Configuration = builder.Build();
            Environment = environment;
            _logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContexts<AdminDbContext>(Configuration);
            services.AddAuthenticationServices<AdminDbContext, UserIdentity, UserIdentityRole>(Environment, Configuration);
            services.AddMvcLocalization();
            //var dic = new Dictionary<string, string>();
            //dic.Add("Url", Configuration["Butterfly:Url"]);
            //dic.Add("Name", Configuration["Butterfly:Name"]);
            services.AddCors(options =>
            {
                options.AddPolicy("spa", config =>
                {
                    config
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .AllowAnyOrigin();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("spa");
            app.Map("/liveness", lapp =>
            {
                lapp.Run(async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                });
            });
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcLocalizationServices();
            app.UseMvcWithDefaultRoute();
        }
    }
}
