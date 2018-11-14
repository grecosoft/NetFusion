using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Configuration;

namespace Solution.Context.WebApi
{
    // Initializes the application's configuration and logging then delegates 
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.overview#bootstrapping---overview
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args) 
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)            
                .UseStartup<Startup>()
                .Build();
        }

        private static void SetupConfiguration(WebHostBuilderContext context, 
            IConfigurationBuilder builder)
        {
            builder.AddDockerDefaultSettings();

            if (EnvironmentConfig.IsDevelopment)
            {
                builder.AddCommandLine(Environment.GetCommandLineArgs());
            }
        }

        private static void SetupLogging(WebHostBuilderContext context, 
            ILoggingBuilder builder)
        {
            var minLogLevel = GetMinLogLevel(context.Configuration);
            builder.ClearProviders();

            if (EnvironmentConfig.IsDevelopment)
            {
                builder.AddDebug().SetMinimumLevel(minLogLevel);
                builder.AddConsole().SetMinimumLevel(minLogLevel);
            }

            // Add additional logger specific to non-development environments.
        }

        // Determines the minimum log level that should be used.  First a configuration value used to specify the 
        // minimum log level is checked.  If present, it will be used.  If not found, the minimum log level based 
        // on the application's execution environment is used.
        private static LogLevel GetMinLogLevel(IConfiguration configuration)
        {
            return configuration.GetValue<LogLevel?>("Logging:MinLogLevel")
                   ?? EnvironmentConfig.EnvironmentMinLogLevel;
        }
    }
}
