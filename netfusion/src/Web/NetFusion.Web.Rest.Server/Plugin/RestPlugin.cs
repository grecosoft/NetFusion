using System;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Plugin;
using NetFusion.Web.Rest.Server.Plugin.Modules;

namespace NetFusion.Web.Rest.Server.Plugin;

public class RestPlugin : PluginBase
{
    public override string PluginId => "77491AC3-31CC-44EC-B508-30E1ED2311CE";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: REST Api";

    public RestPlugin()
    {
        AddModule<ResourceMetaModule>();
        AddModule<RestModule>();
        AddModule<ConcurrencyModule>();

        Description = "Plugin implementing ASP.NET Web Output Formatter used to apply meta-type " +
                      "specific metadata to returned resources.  The plugin implements the HAL meta-type.";
            
        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Web/NetFusion.Web.Rest.Server";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki/web.rest.overview";
    }
}
        
public static class CompositeBuilderExtensions
{
    public static ICompositeContainerBuilder AddRest(this ICompositeContainerBuilder composite)
    {
        ArgumentNullException.ThrowIfNull(composite);

        // Add dependent plugins:
        composite.AddWebMvc();
            
        // Add plugin for Rest API support:
        composite.AddPlugin<RestPlugin>();
            
        return composite;
    }
}