using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Metadata;
using NetFusion.Web.Metadata.Core;

namespace NetFusion.Web.Plugin.Modules;

/// <summary>
/// Plugin module that registers a service used to query ASP.NET controller action metadata.
/// </summary>
public class ApiMetadataModule : PluginModule
{
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddSingleton<IApiMetadataService, ApiMetadataService>();
    }
}