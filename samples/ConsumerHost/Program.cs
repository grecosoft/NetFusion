using Autofac;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Scripting;
using NetFusion.Integration.RabbitMQ;
using NetFusion.Logging.Serilog.Enrichers;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Core;
using Serilog;
using Serilog.Events;

namespace ConsumerHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var typeResolver = new TypeResolver("ConsumerHost", "ConsumerHost", "ExampleApi");
            var loggerFactory = CreateLoggerFactory();

            AppContainer.Create(typeResolver)

                .WithConfig((EnviromentConfig envConfig) => {

                    envConfig.UseDefaultConfiguration();
                })

                .WithConfig((MessagingConfig config) =>
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

        private static ILoggerFactory CreateLoggerFactory()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
                .Verbose()
                .WriteTo.RollingFile("log-{Date}.txt", LogEventLevel.Verbose)
                //.WriteTo.Seq("http://localhost:5341/")
                .Enrich.With<PluginEnricher>()
                .CreateLogger();

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);

            loggerFactory.AddSerilog();

            return loggerFactory;
        }
    }
}
