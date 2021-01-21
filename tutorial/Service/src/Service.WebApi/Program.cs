using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetFusion.Serilog;
using Serilog;
using Serilog.Events;
using Service.WebApi.Plugin;

namespace Service.WebApi
{
    // Initializes the application's configuration and logging then delegates 
    // to the Startup class to initialize HTTP pipeline related settings.
    public class Program
    {
        // Allows changing the minimum log level of the service at runtime.
        private static readonly LogLevelControl LogLevelControl = new LogLevelControl();
        
        public static async Task Main(string[] args)
        {
            IHost webHost = BuildWebHost(args);
            
            var compositeApp = webHost.Services.GetRequiredService<ICompositeApp>();
            var lifetime = webHost.Services.GetRequiredService<IHostApplicationLifetime>();
            
            lifetime.ApplicationStopping.Register(() =>
            {
                compositeApp.Stop();
                Log.CloseAndFlush();
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
                    webBuilder.ConfigureServices(sc =>
                    {
                        // Register Log Level Control so it can be injected into
                        // a service at runtime to change the level.
                        sc.AddLogLevelControl(LogLevelControl);
                    });
                })
                .UseSerilog()
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
            var seqUrl = context.Configuration.GetValue("logging:seqUrl", "http://localhost:5341");

            // Send any serilog configuration issue logs to console.
            Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));
            Serilog.Debugging.SelfLog.Enable(Console.Error);
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LogLevelControl.Switch)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)

                .Enrich.FromLogContext()
                .Enrich.WithCorrelationId()
                .Enrich.WithHostIdentity(WebApiPlugin.HostId, WebApiPlugin.HostName)
                
                .WriteTo.ColoredConsole()
                .WriteTo.Seq(seqUrl)
                .CreateLogger();
        }
    }
}
