using NetFusion.Bootstrap.Container;
using NetFusion.Common.Extensions;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Core;
using NetFusion.Settings.Configs;
using NetFusion.Settings.MongoDB;
using NetFusion.Settings.Strategies;
using Samples.WebHost.App_Start;
using System.Web.Routing;

namespace RefArch.Host
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AppContainer.Create(new[] { "RefArch.*.dll" })
                .WithConfigSection("netFusion", "mongoAppSettings")

                // Eventing Plug-in Configuration.
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

                .Build()
                .Start();

            MvcRouteConfig.RegisterRoutes(RouteTable.Routes);

            var log = AppContainer.Instance.Log.ToJson();
        }

        public void Application_End()
        {
            AppContainer.Instance.Dispose();
        }
    }
}
