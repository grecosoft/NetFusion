using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFusion.AMQP.Plugin;
using NetFusion.Base.Serialization;
using NetFusion.Bootstrap.Container;
using NetFusion.Builder;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Serialization;
using NetFusion.Settings.Plugin;
using Service.Client.Plugin;

namespace Service.Client
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            IHost host = BuildHost();
            
            var compositeApp = host.Services.GetService<ICompositeApp>();
            var lifetime = host.Services.GetService<IApplicationLifetime>();

            lifetime.ApplicationStopping.Register(() =>
            {
                compositeApp.Stop();
            });

            await compositeApp.StartAsync();
            await BuildHost().WaitForShutdownAsync();
        }
        
        private static IHost BuildHost()
        {
            var host = new HostBuilder()
                .ConfigureServices((context, collection) =>
                {
                    collection.CompositeContainer(context.Configuration)
                        .AddSettings()
                        .AddMessaging()
                        .AddRabbitMq()
                        .AddAmqp()
                        .AddRedis()

                        .AddPlugin<ClientPlugin>()
                        .Compose();
                })
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)
                .Build();
            
            return host;
        }

        private static void SetupConfiguration(HostBuilderContext context, 
            IConfigurationBuilder builder)
        {            
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile($"appsettings.json");
        }

        private static void SetupLogging(HostBuilderContext context, 
            ILoggingBuilder builder)
        {
            builder.ClearProviders();

            builder.AddDebug().SetMinimumLevel(LogLevel.Debug);
            builder.AddConsole().SetMinimumLevel(LogLevel.Debug);
        }
    }
}
