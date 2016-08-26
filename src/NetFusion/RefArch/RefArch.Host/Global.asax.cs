using AutofacSerilogIntegration;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Serilog.Core;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Core;
using NetFusion.Settings.Configs;
using NetFusion.Settings.MongoDB;
using NetFusion.Settings.Strategies;
using NetFusion.WebApi.Configs;
using Samples.WebHost.App_Start;
using Serilog;
using System.Web.Routing;

namespace RefArch.Host
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Create logger:
            ILogger logger = CreateLogger();

            AppContainer.Create("RefArch.*.dll")
                .WithConfigSection("netFusion", "mongoAppSettings")

                // Messaging Plug-in Configuration.
                .WithConfig((MessagingConfig config) =>
                {
                    config.AddMessagePublisherType<RabbitMqMessagePublisher>();
                })

                // Configure Settings Plug-in.  This tells the plug-in where to look for
                // injected application settings.
                .WithConfig((NetFusionConfig config) => {

                    config.AddSettingsInitializer(
                        typeof(FileSettingsInitializer<>),
                        typeof(MongoSettingsInitializer<>));
                })

                .WithConfig((GeneralWebApiConfig config) => {

                    config.UseHttpAttributeRoutes = true;
                    config.UseCamalCaseJson = true;
                    config.UseAutofacFilters = true;
                    config.UseJwtSecurityToken = true;
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

            MvcRouteConfig.RegisterRoutes(RouteTable.Routes);
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

        public void Application_End()
        {
            AppContainer.Instance.Stop();
            AppContainer.Instance.Dispose();
        }
    }
}
