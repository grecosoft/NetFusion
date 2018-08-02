using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using System;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.RabbitMQ.Logging;

namespace Demo.WebApi
{
    // Configures the HTTP request pipeline and bootstraps the NetFusion 
    // application container.
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
             services.AddLogging();

            var builtContainer = CreateAppContainer(services, _configuration, _loggerFactory);

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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        // Creates a NetFusion application container used to populate the service-collection 
        // for a set of discovered plug-ins.
        private static IBuiltContainer CreateAppContainer(IServiceCollection services, 
            IConfiguration configuration, 
            ILoggerFactory loggerFactory)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "Demo.WebApi",
                "Demo.*");

            return services.CreateAppBuilder(
                    configuration, 
                    loggerFactory, 
                    typeResolver)

                .Bootstrap(c => {
                    c.WithConfig((MessageDispatchConfig mc) => {
                        mc.AddMessagePublisher<RabbitMqPublisher>();
                    });

                    c.WithConfig((RabbitMqLoggerConfig config) => {
                        config.SetLogFactory(loggerFactory);
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

