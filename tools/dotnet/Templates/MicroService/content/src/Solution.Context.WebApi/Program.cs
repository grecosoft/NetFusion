using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;

namespace Solution.Context.WebApi
{
    // Initializes the application's configuration and logging then delegates 
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IWebHost webHost = BuildWebHost(args);
            
            var compositeApp = webHost.Services.GetService<ICompositeApp>();
            var lifetime = webHost.Services.GetService<IApplicationLifetime>();

            lifetime.ApplicationStopping.Register(() =>
            {
                compositeApp.Stop();
            });
                  
            await compositeApp.StartAsync();
            await webHost.RunAsync();    
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
            builder.AddAppSettings(context.HostingEnvironment);
        }

        private static void SetupLogging(WebHostBuilderContext context, 
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
