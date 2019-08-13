using Demo.App.Plugin;
using Demo.Core.Plugin;
using Demo.Core.Plugin.Configs;
using Demo.Domain.Plugin;
using Demo.Infra.Plugin;
using Demo.WebApi.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Builder;
using NetFusion.Mapping.Plugin;
using NetFusion.Messaging.Plugin;
using NetFusion.MongoDB.Plugin;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Settings.Plugin;
using NetFusion.Web.Mvc.Composite;

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
                .AddMessaging()     
                .AddRabbitMq()
                .AddMapping()
                .AddMongoDb()
                .AddRedis()
                //.AddAmqp()
                
                // This should be added for the validation example showing how to specify custom validation:
                //.InitContainerConfig<ValidationConfig>(config =>
                //{
                //    config.UseValidatorType<CustomObjectValidator>();
                //})
                
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
                .Compose();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        { 
            if (env.IsDevelopment())
            {
                app.UseCompositeQuerying();
            }
            
            app.UseMvc();
        }
    }
}

