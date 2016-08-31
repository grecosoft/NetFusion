using Autofac;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.WebApi.Configs;
using NetFusion.WebApi.Filters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace NetFusion.WebApi.Modules
{
    public class WebApiModule : PluginModule
    {
        private GeneralWebApiConfig GeneralConfig { get; set; }

        public override void Initialize()
        {
            GeneralConfig = Context.Plugin.GetConfig<GeneralWebApiConfig>();
        }

        public override void StartModule(ILifetimeScope scope)
        {
            GlobalConfiguration.Configure(config =>
                OnConfigureWebApiReady(config, scope));
        }

        // When the WebApi runtime is ready for configuration, configure the
        // needed options based on the provided host configuration.
        private void OnConfigureWebApiReady(HttpConfiguration config, ILifetimeScope scope)
        {
            ConfigureSerializerOptions(config);
            ConfigureRoutingOptions(config);
            ConfigureExceptionFilter(config);
            NotifyAllModulesOfWebApiReady(config, AppContainer.Instance.Services);
        }

        private void ConfigureRoutingOptions(HttpConfiguration config)
        {
            if (GeneralConfig.UseHttpAttributeRoutes)
            {
                config.MapHttpAttributeRoutes();
            }
        }

        private void ConfigureSerializerOptions(HttpConfiguration config)
        {
            if (GeneralConfig.UseCamalCaseJson)
            {
                var jsonFormatter = config.Formatters.JsonFormatter;
                if (jsonFormatter != null)
                {
                    jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                }
            }
        }

        private void ConfigureExceptionFilter(HttpConfiguration config)
        {
            config.Filters.Add(new NetFusionExceptionFilter());
        }

        // For all other plug-in modules implementing the IWebApiConfiguration interface
        // allow them a change to add any needed WebApi configurations.
        private void NotifyAllModulesOfWebApiReady(HttpConfiguration config, IContainer container)
        {
            var apiConfigModules = this.Context.AllPluginModules.Where(m => m != this)
               .OfType<IWebApiConfiguration>()
               .ToList();

            apiConfigModules.ForEach(m => m.OnConfigureWebApiReady(config, container));
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var webApiLog = new Dictionary<string, object>();

            moduleLog["WebApi"] = webApiLog;
            LogConfigSettings(webApiLog);
            LogControllers(webApiLog);
        }

        private void LogConfigSettings(IDictionary<string, object> webApiLog)
        {
            webApiLog[nameof(GeneralWebApiConfig)] = GeneralConfig;
        }

        private void LogControllers(Dictionary<string, object> webApiLog)
        {
            var allPluginTypes = Context.GetPluginTypesFrom();

            webApiLog["Controllers"] = allPluginTypes.Where(t => t.IsDerivedFrom<IHttpController>())
                .Select(t => Context.GetPluginType(t))
                .ToLookup(k => k.Plugin.Manifest.Name, v => v.Type.FullName)
                .Select(g => new {
                    Plugin = g.Key,
                    Controllers = g
                });
        }
    }
}
