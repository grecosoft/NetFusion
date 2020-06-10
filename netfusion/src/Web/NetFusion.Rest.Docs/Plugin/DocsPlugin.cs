using System;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Plugin.Modules;
using NetFusion.Web.Mvc.Plugin;

namespace NetFusion.Rest.Docs.Plugin
{
    public class DocsPlugin : PluginBase
    {
        public override string PluginId => "77491AC3-31CC-44EC-B508-30E1ED2311CE";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: REST Plugin";

        public DocsPlugin()
        {
            AddModule<DocModule>();

            Description = "Plugin implementing management and querying of REST/HAL API documentation";
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Web/NetFusion.Rest.Docs";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki";
        }
    }
        
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddRestDocs(this ICompositeContainerBuilder composite)
        {
            if (composite == null) throw new ArgumentNullException(nameof(composite));
            
            // Add dependent plugins:
            composite.AddWebMvc();
            
            // Add plugin for Rest API support:
            composite.AddPlugin<DocsPlugin>();
            
            return composite;
        }
    }
}