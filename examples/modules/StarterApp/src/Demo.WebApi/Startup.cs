using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using System;
using NetFusion.Bootstrap.Configuration;

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
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)); 
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Support REST/HAL based API responses.
            services.AddMvc(options => {

            });

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
            app.UseMvc();
        }

        // Creates a NetFusion application container used to populate the service-collection 
        // for a set of discovered plug-ins.
        private IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            // Creates an instance of a type resolver that will look for plug-ins within 
            // the assemblies matching the passed patterns.
            var typeResolver = new TypeResolver(
                "Demo.WebApi",
                "Demo.*");

            return services.CreateAppBuilder(configuration, loggerFactory, typeResolver)
                .Bootstrap(c => {

                    c.WithServices(reg =>
                     {
                         //  Any additional services or overrides can be registered here.
                     });
                })
                .Build();
        }

        private static void OnShutdown()
        {
            AppContainer.Instance.Stop();
        }
    }
}
