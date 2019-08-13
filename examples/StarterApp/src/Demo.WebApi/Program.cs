using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;

namespace Demo.WebApi
{
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
            
            Console.WriteLine("Web Api Services Stopped.");
        }
        
        private static IWebHost BuildWebHost(string[] args) 
        {
            return WebHost.CreateDefaultBuilder(args)
              .ConfigureAppConfiguration((context, builder) => { 
                  
                  SetupConfiguration(builder, context.HostingEnvironment);
              })
              .ConfigureLogging(SetupLogging)
              .ConfigureServices(services =>
              {

              })
              .UseStartup<Startup>()
              .Build();
        }

        private static void SetupConfiguration(IConfigurationBuilder builder, IHostingEnvironment hostingEnv)
        {
            builder.Sources.Clear();
            builder.AddAppSettings(hostingEnv);

            if (hostingEnv.IsDevelopment())
            {
                builder.AddCommandLine(Environment.GetCommandLineArgs());
            }
        }

        private static void SetupLogging(WebHostBuilderContext context, ILoggingBuilder builder)
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

