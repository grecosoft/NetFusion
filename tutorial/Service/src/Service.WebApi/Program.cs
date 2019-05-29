using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;

namespace Service.WebApi
{
    // Initializes the application's configuration and logging then delegates 
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.overview#bootstrapping---overview
        public static async Task Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);

            // Start all of the plugin modules:
            await CompositeContainer.Instance.StartAsync();
            await webHost.RunAsync();

            // Stop all plugin modules:
            await CompositeContainer.Instance.StopAsync();
        }

        private static IWebHost BuildWebHost(string[] args) 
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseEnvironment(EnvironmentName.Development)
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)    
                .UseStartup<Startup>()
                .Build();
        }

        private static void SetupConfiguration(WebHostBuilderContext context, 
            IConfigurationBuilder builder)
        {
            builder.AddAppSettings(context.HostingEnvironment);
        }

        private static void SetupLogging(WebHostBuilderContext context, 
            ILoggingBuilder builder)
        {
            builder.ClearProviders();

            if (context.HostingEnvironment.IsDevelopment())
            {
                builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
            }
            else
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Warning);
            }
        }
    }
}
