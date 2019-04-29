﻿using NetFusion.AMQP.Plugin;
using NetFusion.Bootstrap.Refactors;
using NetFusion.Messaging.Plugin;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.Redis.Plugin;
using NetFusion.Settings.Plugin;
using Service.Client.Plugin;
using Service.Domain.Plugin;

namespace Service.Client
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NetFusion.AMQP.Subscriber;
    using NetFusion.Base.Serialization;
    using NetFusion.Bootstrap.Configuration;
    using NetFusion.Bootstrap.Container;
    using NetFusion.Serialization;

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
                    var loggerFactory = new LoggerFactory();
                    loggerFactory.AddConsole();
                    
                    collection.AddSingleton<ISerializationManager, SerializationManager>();
                    
                    collection.CompositeAppBuilder(loggerFactory, context.Configuration)
                        .AddSettings()
                        .AddMessaging()
                        .AddRabbitMq()
                        .AddAmqp()
                        .AddRedis()

                        .AddPlugin<ClientPlugin>()
                        .Build()
                        .Start();
                })
                .ConfigureAppConfiguration(SetupConfiguration)
                .ConfigureLogging(SetupLogging)
                .Build();
            
            return host;
        }

        private static void SetupConfiguration(HostBuilderContext context, 
            IConfigurationBuilder builder)
        {            
            builder.AddAppSettings();
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
