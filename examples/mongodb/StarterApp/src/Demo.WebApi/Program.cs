using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Configuration;

namespace Demo.WebApi
{
    // Initializes the application's configuration and logging then delegates
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static IWebHost BuildWebHost(string[] args)
        {
            var configuration = CreateConfiguration(args);

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(configuration)
                .ConfigureLogging((context, logging) => SetupLogging(logging, configuration))
                .UseStartup<Startup>()
                .Build();
        }

        private static IConfiguration CreateConfiguration(string[] args)
        {
            var builder = new ConfigurationBuilder();
            builder.AddAppSettings();

            if (EnvironmentConfig.IsDevelopment)
            {
                builder.AddCommandLine(args);
            }

            return builder.Build();
        }

        private static void SetupLogging(ILoggingBuilder logging, IConfiguration configuration)
        {
            var minLogLevel = GetMinLogLevel(configuration);
            logging.ClearProviders();

            if (EnvironmentConfig.IsDevelopment)
            {
                logging.AddDebug().SetMinimumLevel(minLogLevel);
                logging.AddConsole().SetMinimumLevel(minLogLevel);
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