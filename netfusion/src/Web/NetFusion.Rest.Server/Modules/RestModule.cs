using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
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

        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper>(sp => {
                var context = sp.GetRequiredService<IActionContextAccessor>();
                var urlFactory = sp.GetRequiredService<IUrlHelperFactory>();
                return urlFactory.GetUrlHelper(context.ActionContext);
            });

            services.AddSingleton<IHalEmbededResourceContext, HalEmbeddedResourceContext>();
            services.AddScoped<EnvironmentSettings>();
        }
    }
}
