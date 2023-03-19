using System;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.Server.Hal;
using NetFusion.Web.Rest.Server.Hal.Core;

namespace NetFusion.Web.Rest.Server.Plugin.Modules;

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

            if (context.ActionContext == null)
            {
                throw new NullReferenceException("Action Context not Initialized");
            }
                
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