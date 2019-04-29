using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Server.Hal;
using System;
using NetFusion.AMQP.Plugin;
using NetFusion.Bootstrap.Configuration;
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

namespace Service.WebApi
{
    using NetFusion.Base.Serialization;
    using NetFusion.Serialization;

    // Configures the HTTP request pipeline and bootstraps the NetFusion 
    // application container.
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)); 
        }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddSingleton<ISerializationManager, SerializationManager>();
            
            services.CompositeAppBuilder(_loggerFactory, _configuration)
                .AddSettings()
                .AddMessaging()
                .AddMongoDb()
                .AddRabbitMq()
                .AddRedis()
                .AddAmqp()
                .AddWebMvc(c =>
                {
                    c.EnableRouteMetadata = true;
                    c.UseServices(services);
                })
                .AddRest()
                .AddPlugin<DomainPlugin>()
                .AddPlugin<AppPlugin>()
                .AddPlugin<InfraPlugin>()
                .AddPlugin<WebApiPlugin>()
                .Build()
                .Start();
                
             
            
            
            
            
            
            if (EnvironmentConfig.IsDevelopment)
            {
                services.AddCors();                
            }

            // Support REST/HAL based API responses.
            services.AddMvc(options => {
                options.UseHalFormatter();
            });

            // Create and NetFusion application container based on Microsoft's abstractions:
            //var builtContainer = CreateAppContainer(services, _configuration, _loggerFactory);

            // Start all modules in the application container and return created service-provider.
            // If an open-source DI container is needed, this is where it would be populated and returned.
            //builtContainer.Start();
            //return builtContainer.ServiceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            IApplicationLifetime applicationLifetime)
        {
            // This registers a method to be called when the Web Application is stopped.
            // In this case, we want to delegate to the NetFusion AppContainer so it can
            // safely stopped.
            applicationLifetime.ApplicationStopping.Register(OnShutdown);

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

        private static void OnShutdown()
        {
            //AppContainer.Instance.Dispose();
        }
    }
}

