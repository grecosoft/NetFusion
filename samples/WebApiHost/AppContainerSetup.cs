using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Configuration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Serilog.Enrichers;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.MongoDB.Metadata;
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

            var configuration = CreateConfiguration();
            var loggerFactory = CreateLoggerFactory(configuration);

            AppContainer.Create(typeResolver)

                .WithConfig((EnvironmentConfig envConfig) => {

                    envConfig.UseConfiguration(configuration);
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
