using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Hal;
using NetFusion.Rest.Server.Plugin.Configs;

namespace NetFusion.Rest.Server.Plugin.Modules
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
        
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped(sp => {
                var context = sp.GetRequiredService<IActionContextAccessor>();
                var urlFactory = sp.GetRequiredService<IUrlHelperFactory>();
                return urlFactory.GetUrlHelper(context.ActionContext);
            });

            services.AddSingleton<IHalEmbeddedResourceContext, HalEmbeddedResourceContext>();
            
            // Support REST/HAL based API responses.
            services.AddMvc(options => {
                options.UseHalFormatter();
            });
        }
    }
}    
