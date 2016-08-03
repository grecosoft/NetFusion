using AutofacSerilogIntegration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Serilog.Core;
using NetFusion.Messaging.Config;
using NetFusion.Settings.Configs;
using NetFusion.Settings.Strategies;
using Serilog;
using System;

namespace RefArch.Subscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new Program();
            host.Bootstrap();

            Console.WriteLine("Client Started");
        }

        private void Bootstrap()
        {
            // Create logger:
            ILogger logger = CreateLogger();

            AppContainer.Create("RefArch.*.exe", "RefArch.*.dll" )
                .WithConfigSection("netFusion", "mongoAppSettings")

                .WithConfig((NetFusionConfig config) => {

                    config.AddSettingsInitializer(
                        typeof(FileSettingsInitializer<>));
                })

                .WithConfig((MessagingConfig config) =>
                {
                    // Only required if client will be publishing events.
                    //config.AddEventPublisherType<RabbitMqEventPublisher>();
                })

                .WithConfig((LoggerConfig config) =>
                {
                    config.LogExceptions = true;
                    config.SetLogger(new SerilogPluginLogger(logger));
                })

                .WithConfig((AutofacRegistrationConfig config) =>
                {
                    config.Build = builder =>
                    {
                        // Add any components created during startup.
                        builder.RegisterLogger(logger);
                    };
                })

                .Build()
                .Start();
        }

        private ILogger CreateLogger()
        {
            var logConfig = new LoggerConfiguration()
                .WriteTo.Seq("http://localhost:5341");

            logConfig
                .Enrich.With<PluginEnricher>()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithThreadId();

            logConfig.MinimumLevel.Verbose();
            return logConfig.CreateLogger();
        }
    }
}
