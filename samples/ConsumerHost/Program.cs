using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Serilog.Enrichers;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.MongoDB.Metadata;
using Serilog;
using Serilog.Events;

namespace ConsumerHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var typeResolver = new TypeResolver("ConsumerHost", "ConsumerHost", "ExampleApi");

            var configuration = CreateConfiguration();
            var loggerFactory = CreateLoggerFactory(configuration);

            AppContainer.Create(typeResolver)

                .WithConfig((EnvironmentConfig envConfig) => {

                    envConfig.UseConfiguration(configuration);
                })

                .WithConfig((MessageDispatchConfig config) =>
                {
                    // Only required if client will be publishing events.
                    config.AddMessagePublisher<RabbitMqMessagePublisher>();
                })

                .WithConfig((LoggerConfig config) =>
                {
                    config.UseLoggerFactory(loggerFactory);
                    config.LogExceptions = true;
                })

                .WithConfig((AutofacRegistrationConfig config) =>
                {
                    config.Build = builder =>
                    {
                        builder.RegisterType<NullEntityScriptingService>()
                            .As<IEntityScriptingService>()
                            .SingleInstance();

                        builder.RegisterType<BrokerMetaRepository>()
                            .As<IBrokerMetaRepository>()
                            .SingleInstance();
                    };
                })

                .Build()
                .Start();
        }

        private static IConfiguration CreateConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddEnvironmentVariables();
            configBuilder.AddDefaultAppSettings();

            return configBuilder.Build();
        }

        private static ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
                .Verbose()
                .WriteTo.RollingFile("log-{Date}.txt", LogEventLevel.Verbose)
                //.WriteTo.Seq("http://localhost:5341/")
                .Enrich.With<NetFusionLogEnricher>()
                .CreateLogger();

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);

            loggerFactory.AddSerilog();

            return loggerFactory;
        }
    }
}
