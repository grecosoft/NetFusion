using Autofac;
using Autofac.Integration.WebApi;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.WebApi.Configs;
using NetFusion.WebApi.Handlers;
using System.Web.Http;

namespace NetFusion.WebApi.Modules
{
    /// <summary>
    /// The configuration module for WebApi centric configurations
    /// for AutoFac.
    /// </summary>
    public class AutofacModule : PluginModule,
        IWebApiConfiguration
    {
        private GeneralWebApiConfig GeneralConfig { get; set; }

        public override void Initialize()
        {
            GeneralConfig = Context.Plugin.GetConfig<GeneralWebApiConfig>();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            ConfigureAutofacExtensions(builder);
        }

        private void ConfigureAutofacExtensions(ContainerBuilder builder)
        {
            ConfigureAutofacFilters(builder);
            RegisterControllersWithAutofac(builder);
        }

        private void ConfigureAutofacFilters(ContainerBuilder builder)
        {
            if (GeneralConfig.UseAutofacFilters)
            {
                builder.RegisterWebApiFilterProvider(GlobalConfiguration.Configuration);
            }
        }

        private void RegisterControllersWithAutofac(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(this.Context.AllCorePluginTypes.ContainingAssemblies());
            builder.RegisterApiControllers(this.Context.AllAppPluginTypes.ContainingAssemblies());
        }

        public void OnConfigureWebApiReady(HttpConfiguration config, IContainer container)
        {
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            
            if (GeneralConfig.UseJwtSecurityToken)
            {
                config.MessageHandlers.Add(container.Resolve<JwtTokenValidationHandler>());
            }
        }
    }
}
