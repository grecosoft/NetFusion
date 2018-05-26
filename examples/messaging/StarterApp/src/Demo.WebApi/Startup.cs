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

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _loggerFactory = CreateLoggerFactory(configuration);
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
        private IBuiltContainer CreateAppContainer(IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory)
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
            	.Build();
        }
        
        private static ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
        {
            var minLogLevel = GetMinLogLevel(configuration);
            var loggerFactory = new LoggerFactory();
                
            if (EnvironmentConfig.IsDevelopment)
            {
                loggerFactory
                    .AddDebug(minLogLevel)
                    .AddConsole(minLogLevel);
            }
            else // PRODUCTION and STAGING/TEST
            {
            
            }

            return loggerFactory;
        }
        
        // Determines the minimum log level that should be used.  First a configuration value used to specify the 
        // minimum log level is checked.  If present, it will be used.  If not found, the minimum log level based 
        // on the application's execution environment is used.
        private static LogLevel GetMinLogLevel(IConfiguration configuration)
        {
            return configuration.GetValue<LogLevel?>("Logging:MinLogLevel")
                   ?? EnvironmentConfig.EnvironmentMinLogLevel;
        }

        private static void OnShutdown()
        {
            AppContainer.Instance.Stop();
        }
    }
}

