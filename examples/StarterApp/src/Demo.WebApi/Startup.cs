using Demo.App.Plugin;
using Demo.Core.Plugin;
using Demo.Core.Plugin.Configs;
using Demo.Domain.Plugin;
using Demo.Infra;
using Demo.Infra.Plugin;
using Demo.WebApi.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Settings.Plugin;

namespace Demo.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion application container.
    public class Startup
    {
        // Microsoft Abstractions:
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHostingEnvironment _hostingEnv;

        public Startup(IConfiguration configuration, ILogger<CompositeContainer> logger, ILoggerFactory loggerFactory, IHostingEnvironment hostingEnv)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _hostingEnv = hostingEnv;
            
            logger.LogWarning("sfsdfsdf");
        }
        
        public void ConfigureServices(IServiceCollection services)
        {        
            services.CompositeAppBuilder(_loggerFactory, _configuration)
                
                .AddSettings()
                .AddMessaging()     
                .AddRabbitMq()
                
                // !! THIS SHOULD ONLY BE UNCOMMENTED FOR THE CUSTOM MESSAGE-PUBLISHER OR ENRICHER EXAMPLES. !!
                //.InitPluginConfig<MessageDispatchConfig>(config =>
                //{
                //    config.ClearPublishers();
                //    config.AddPublisher<ExamplePublisher>();
                //    config.AddEnricher<MachineNameEnricher>();
                //})

                .AddPlugin<CorePlugin>()
                .InitPluginConfig<ValidRangeConfig>(
                    range => range.SetRange(1000, 5000))
                
                .AddPlugin<InfraPlugin>()
                .AddPlugin<AppPlugin>()
                .AddPlugin<DomainPlugin>()
                .AddPlugin<WebApiPlugin>()
                .Build();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            app.UseMvc();
        }
    }
}

