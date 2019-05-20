using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Builder;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Web.Mvc.Plugin;
using Solution.Context.App.Plugin;
using Solution.Context.Domain.Plugin;
using Solution.Context.Infra.Plugin;
using Solution.Context.WebApi.Plugin;

namespace Solution.Context.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion application container.
    public class Startup
    {
        // Microsoft Abstractions:
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnv)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _hostingEnv = hostingEnv;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCors();
        
            services.CompositeAppBuilder(_loggerFactory, _configuration)
                .AddWebMvc(config =>
                {
                    config.EnableRouteMetadata = true;
                    config.UseServices(services);
                })
                .AddRest()
 
                .AddPlugin<InfraPlugin>()
                .AddPlugin<AppPlugin>()
                .AddPlugin<DomainPlugin>()
                .AddPlugin<WebApiPlugin>()
                .Build();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            app.UseAuthentication();

            string viewerUrl = _configuration.GetValue<string>("Netfusion:ViewerUrl");
            if (! string.IsNullOrWhiteSpace(viewerUrl))
            {
                app.UseCors(builder => builder.WithOrigins(viewerUrl)
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("WWW-Authenticate","resource-404")
                    .AllowAnyHeader());
            }

            app.UseMvc();
        }
    }
}