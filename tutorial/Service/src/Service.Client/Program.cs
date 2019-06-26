using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetFusion.AMQP.Plugin;
using NetFusion.Base.Serialization;
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
            await BuildHost().WaitForShutdownAsync();
            
          //  AppContainer.Instance.Dispose();
        }
        
        private static IHost BuildHost()
        {
            var host = new HostBuilder()
                .ConfigureServices((context, collection) =>
                {
                    collection.AddSingleton<ISerializationManager, SerializationManager>();

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
            builder.AddJsonFile($"appsettings.json", optional: true, reloadOnChange: true);
        }

        private static void SetupLogging(HostBuilderContext context, 
            ILoggingBuilder builder)
        {
            builder.ClearProviders();

//            if (EnvironmentConfig.IsDevelopment)
//            {
                builder.AddDebug().SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace);
//            }

            // Add additional logger specific to non-development environments.
        }
        
    }
}
