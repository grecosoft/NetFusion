using System;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Plugin.Configs;
using NetFusion.Rest.Docs.Plugin.Modules;
using NetFusion.Web.Mvc.Plugin;

namespace NetFusion.Rest.Docs.Plugin
{
    public class DocsPlugin : PluginBase
    {
        public override string PluginId => "B792D448-278B-439E-B3B5-35F2AECC2232";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: REST Doc Plugin";

        public DocsPlugin()
        {
            AddConfig<RestDocConfig>();
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