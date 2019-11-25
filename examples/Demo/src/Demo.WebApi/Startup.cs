using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Builder;
using NetFusion.Messaging.Plugin;
using NetFusion.Rest.Server.Plugin;
using NetFusion.Settings.Plugin;
using NetFusion.Web.Mvc.Plugin;
using Demo.App.Plugin;
using Demo.Core.Plugin;
using Demo.Core.Plugin.Configs;
using Demo.Domain.Plugin;
using Demo.Infra;
using Demo.Infra.Plugin;
using Demo.WebApi.Plugin;
using NetFusion.MongoDB.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Mapping.Plugin;
using NetFusion.Roslyn.Plugin;

namespace Demo.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion application container.
    public class Startup
    {
        // Microsoft Abstractions:
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.CompositeContainer(_configuration)
                .AddSettings()
                .AddMapping()
                .AddMessaging()
                .AddRabbitMq()
                .AddRedis()
                .AddRoslyn()
                .AddMongoDb()

                .AddRest()
                .AddWebMvc(config =>
                {
                    config.EnableRouteMetadata = true;
                })


//                Enable for the Messaging-Publisher/Enricher examples ONLY                
//                .InitPluginConfig<MessageDispatchConfig>(config =>
//                    config.AddPublisher<ExamplePublisher>();
//                    config.AddEnricher<MachineNameEnricher>();
//                })
 
                .InitPluginConfig<QueryDispatchConfig>(c =>
                {
                    c.AddFilter<TimeQueryFilter>();
                })

                .AddPlugin<CorePlugin>()
                .InitPluginConfig<ValidRangeConfig>(
                   range => range.SetRange(1000, 5000))

                .AddPlugin<InfraPlugin>()
                .AddPlugin<AppPlugin>()
                .AddPlugin<DomainPlugin>()
                .AddPlugin<WebApiPlugin>()
                .Compose(config =>
                {
                    // config.AddSingleton<IValidationService, CustomValidationService>();
                });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddCors();
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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