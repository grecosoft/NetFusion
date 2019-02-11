using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Rest.Server.Hal;
using NetFusion.Web.Mvc;
using NetFusion.Web.Mvc.Composite;
using System;
using NetFusion.Bootstrap.Configuration;

namespace Service.WebApi
{
    using NetFusion.AMQP.Publisher;
    using NetFusion.AMQP.Subscriber;
    using NetFusion.Base.Serialization;
    using NetFusion.Messaging.Config;
    using NetFusion.RabbitMQ.Logging;
    using NetFusion.RabbitMQ.Publisher;
    using NetFusion.Redis.Publisher;
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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (EnvironmentConfig.IsDevelopment)
            {
                services.AddCors();                
            }

            // Support REST/HAL based API responses.
            services.AddMvc(options => {
                options.UseHalFormatter();
            });
            
            services.AddHostedService<AmqpSubscriberHostedService>();

            // Create and NetFusion application container based on Microsoft's abstractions:
            var builtContainer = CreateAppContainer(services, _configuration, _loggerFactory);

            // Start all modules in the application container and return created service-provider.
            // If an open-source DI container is needed, this is where it would be populated and returned.
            builtContainer.Start();
            return builtContainer.ServiceProvider;
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

        // Creates a NetFusion application container used to populate the service-collection 
        // for a set of discovered plug-ins.
        private IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "Service.WebApi",
                "Service.*");

            return services.CreateAppBuilder(configuration, loggerFactory, typeResolver)
                .Bootstrap(c => {

                    c.WithConfig((WebMvcConfig config) =>
                     {
                         config.EnableRouteMetadata = true;
                         config.UseServices(services);
                     })
                     .WithConfig((MessageDispatchConfig mc) => {
                        mc.AddMessagePublisher<RabbitMqPublisher>();
                        mc.AddMessagePublisher<RedisPublisher>();
                        mc.AddMessagePublisher<HostMessagePublisher>();   
                     })
                     .WithConfig((RabbitMqLoggerConfig config) => {
                        config.SetLogFactory(loggerFactory);
                     })
                     .WithServices(reg =>
                     {
                         reg.AddSingleton<ISerializationManager, SerializationManager>();
                         //  Additional services or overrides can be registered here.
                     });
                })
                .Build();
        }

        private static void OnShutdown()
        {
            AppContainer.Instance.Dispose();
        }
    }
}

