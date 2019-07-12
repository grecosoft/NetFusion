﻿using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Hal;

namespace NetFusion.Rest.Server.Plugin.Modules
{
    /// <summary>
    /// Registers services used by the implementation.
    /// </summary>
    public class RestModule : PluginModule
    {
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Adds the IUrlHelper service to the container.  This is delegated to
            // by the plugin to determine URLs corresponding to route information.
            services.AddScoped(sp => {
                var context = sp.GetRequiredService<IActionContextAccessor>();
                var urlFactory = sp.GetRequiredService<IUrlHelperFactory>();
                return urlFactory.GetUrlHelper(context.ActionContext);
            });

            // This service can be injected to determine if the client has
            // specified a specific set of embedded resources be returned.
            services.AddSingleton<IHalEmbeddedResourceContext, HalEmbeddedResourceContext>();
            
            // Support REST/HAL based API responses.
            services.AddMvc(options => {
                options.UseHalFormatter();
            });
        }
    }
}    
