using System;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Plugin;
using NetFusion.Web.Rest.Docs.Plugin.Configs;
using NetFusion.Web.Rest.Docs.Plugin.Modules;
using NetFusion.Web.Rest.Server.Plugin;

namespace NetFusion.Web.Rest.Docs.Plugin;

public class DocsPlugin : PluginBase
{
    public override string PluginId => "B792D448-278B-439E-B3B5-35F2AECC2232";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: REST Documentation";

    public DocsPlugin()
    {
        AddConfig<RestDocConfig>();
        AddModule<DocModule>();

        Description = "Plugin implementing querying of REST/HAL API documentation";
            
        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Web/NetFusion.Web.Rest.Docs";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki";
    }
}
        
public static class CompositeBuilderExtensions
{
    /// <summary>
    /// Adds the REST Documentation Plugin to the composite application container.
    /// </summary>
    /// <param name="composite">The composite container builder.</param>
    /// <returns>Reference to the composite container builder.</returns>
    public static ICompositeContainerBuilder AddRestDocs(this ICompositeContainerBuilder composite)
    {
        if (composite == null) throw new ArgumentNullException(nameof(composite));

        // Add dependent plugins:
        composite.AddWebMvc();
        composite.AddRest();
            
        // Add plugin for Rest API Documentation support:
        composite.AddPlugin<DocsPlugin>();
            
        return composite;
    }
}