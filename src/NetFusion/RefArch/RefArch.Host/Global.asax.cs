﻿using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Core;
using NetFusion.Settings.Configs;
using NetFusion.Settings.MongoDB;
using NetFusion.Settings.Strategies;
using NetFusion.WebApi.Configs;
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

                .WithConfig((GeneralWebApiConfig config) => {

                    config.UseHttpAttributeRoutes = true;
                    config.UseCamalCaseJson = true;
                    config.UseAutofacFilters = true;
                    config.UseJwtSecurityToken = true;
                })

                .Build()
                .Start();

            MvcRouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        public void Application_End()
        {
            AppContainer.Instance.Stop();
            AppContainer.Instance.Dispose();
        }
    }
}
