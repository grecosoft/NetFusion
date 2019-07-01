using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using NetFusion.AMQP.Plugin;
using NetFusion.Messaging.Plugin;
using NetFusion.MongoDB.Plugin;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Settings.Plugin;
using NetFusion.Web.Mvc.Plugin;
using Service.App.Plugin;
using Service.Domain.Plugin;
using Service.Infra.Plugin;
using Service.WebApi.Plugin;
using NetFusion.Builder;
using Service.App.Services;

namespace Service.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion 
    // application container.
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnv)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _hostingEnv = hostingEnv ?? throw new ArgumentNullException(nameof(hostingEnv));
        }

        public void ConfigureServices(IServiceCollection services)
        {         
            services.CompositeContainer(_configuration)
                // Add common plugins:
                .AddSettings()
                .AddMessaging()
                
                // Add technology specific plugins:
                .AddMongoDb()
                .AddRabbitMq()
                .AddRedis()
                .AddAmqp()
                .AddWebMvc(c =>
                {
                    c.EnableRouteMetadata = true;
                })
                .AddRest()
                
                // Add application centric plugins:
                .AddPlugin<DomainPlugin>()
                .AddPlugin<AppPlugin>()
                .AddPlugin<InfraPlugin>()
                .AddPlugin<WebApiPlugin>()
                .Compose();
               
            if (_hostingEnv.IsDevelopment())
            {
                services.AddCors();                
            }

            services.AddMvc();
            services.AddSingleton(InMemoryScripting.LoadSensorScript());
        }

        public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            app.UseAuthentication();
          
            // Add additional middleware here.
            
            string viewerUrl = _configuration.GetValue<string>("Netfusion:ViewerUrl");
            if (! string.IsNullOrWhiteSpace(viewerUrl))
            {
                app.UseCors(builder => builder.WithOrigins(viewerUrl)
                    .AllowAnyMethod()
                    .WithExposedHeaders("WWW-Authenticate")
                    .AllowAnyHeader());
            }
           
            app.UseMvc();
        }
    }
}

