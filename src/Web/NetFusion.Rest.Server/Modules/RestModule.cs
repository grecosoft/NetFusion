using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Hal;

namespace NetFusion.Rest.Server.Modules
{
    /// <summary>
    /// Registers services used by the implementation.
    /// </summary>
    public class RestModule : PluginModule,
        IRestModule
    {
        private RestApiConfig _config;

        public override void Initialize()
        {
            _config = Context.Plugin.GetConfig<RestApiConfig>();
        }

        public string GetControllerSuffix() => _config.ControllerSuffix;    
        public string GetTypeScriptDirectoryName() => _config.TypeScriptDirectoryName;
        public string GetControllerDocDirectoryName() => _config.ControllerDocDirectoryName;

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();

            builder.Register<IUrlHelper>(c =>
            {
                var context = c.Resolve<IActionContextAccessor>();
                var urlFactory = c.Resolve<IUrlHelperFactory>();
                return urlFactory.GetUrlHelper(context.ActionContext);

            }).InstancePerLifetimeScope();

            builder.RegisterType<HalEmbeddedResourceContext>()
                .As<IHalEmbededResourceContext>()
                .SingleInstance();

            builder.RegisterType<EnvironmentSettings>()
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
