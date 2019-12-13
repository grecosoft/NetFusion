using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;

namespace Demo.WebApi
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
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)
                .Build();
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
                builder.AddConsole().SetMinimumLevel(LogLevel.Warning);
            }
        }
    }
}
