using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Scripting;
using NetFusion.Integration.RabbitMQ;
using NetFusion.Logging.Serilog.Enrichers;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Enrichers;
using NetFusion.RabbitMQ.Core;
using NetFusion.Web.Mvc;
using Serilog;
using Serilog.Events;

namespace WebApiHost
{
    public static class AppContainerSetup
    {
        public static void Bootstrap(IServiceCollection services)
        {
            var typeResolver = new TypeResolver("WebApiHost", "ExampleApi", "NetFusion.*", "NetFusion.*.*");
            var loggerFactory = CreateLoggerFactory();

            AppContainer.Create(typeResolver)

                .WithConfig((EnvironmentConfig envConfig) => {

                    envConfig.UseDefaultConfiguration();
                })

                .WithConfig((MessagingConfig config) =>
                {
                    // Only required if client will be publishing events.
                    config.AddMessagePublisher<RabbitMqMessagePublisher>();
                    
                    // Add additional enrichers or clear existing then add specific enrichers.
                })

                .WithConfig((LoggerConfig config) =>
                {
                    config.UseLoggerFactory(loggerFactory);
                    config.LogExceptions = true;
                })

                .WithConfig((WebMvcConfig config) =>
                {
                    config.EnableRouteMetadata = true;
                    config.UseServices(services);
                })

                .WithConfig((AutofacRegistrationConfig config) =>
                {
                    config.Build = builder =>
                    {
                        builder.Populate(services);

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
                .WriteTo.Seq("http://localhost:5341/")
                .Enrich.With<PluginEnricher>()
                .CreateLogger();

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddDebug(LogLevel.Trace);

            loggerFactory.AddSerilog();

            return loggerFactory;
        }
    }
}
