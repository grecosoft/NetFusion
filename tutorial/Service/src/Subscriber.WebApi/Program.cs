using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Subscriber.WebApi
{
    // Initializes the application's configuration and logging then delegates 
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IHost webHost = BuildWebHost(args);
            
            var compositeApp = webHost.Services.GetService<ICompositeApp>();
            var lifetime = webHost.Services.GetService<IHostApplicationLifetime>();

            lifetime.ApplicationStopping.Register(() =>
            {
                compositeApp.Stop();
            });
                  
            await compositeApp.StartAsync();
            await webHost.RunAsync();    
        }

        private static IHost BuildWebHost(string[] args) 
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).Build();
        }

        private static void SetupConfiguration(HostBuilderContext context, 
            IConfigurationBuilder builder)
        {
            builder.AddAppSettings(context.HostingEnvironment);
        }

        private static void SetupLogging(HostBuilderContext context, 
            ILoggingBuilder builder)
        {
            builder.ClearProviders();

            if (context.HostingEnvironment.IsDevelopment())
            {
                builder.AddDebug().SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
            }
            else
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Information);
            }
        }
    }
}