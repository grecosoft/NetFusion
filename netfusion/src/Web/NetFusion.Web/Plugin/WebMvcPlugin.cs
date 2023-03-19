using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Plugin.Modules;

namespace NetFusion.Web.Plugin;

public class WebMvcPlugin : PluginBase
{
    public override string PluginId => "3C757DE1-48E1-452D-959A-01C8961B43D8";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: Web-MVC";
        
    public WebMvcPlugin()
    {
        AddModule<ApiMetadataModule>();

        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Web/NetFusion.Web";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki";
    }
}
    
public static class CompositeBuilderExtensions
{
    public static ICompositeContainerBuilder AddWebMvc(this ICompositeContainerBuilder composite)
    {
        // Add the MVC Plugin:
        composite.AddPlugin<WebMvcPlugin>();
        return composite;
    }
}