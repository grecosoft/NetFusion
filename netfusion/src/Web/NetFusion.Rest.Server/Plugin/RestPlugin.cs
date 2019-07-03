using System;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Server.Plugin.Configs;
using NetFusion.Rest.Server.Plugin.Modules;

namespace NetFusion.Rest.Server.Plugin
{
    public class RestPlugin : PluginBase
    {
        public override string PluginId => "77491AC3-31CC-44EC-B508-30E1ED2311CE";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: REST/HAL Plugin";

        public RestPlugin()
        {
            AddConfig<RestApiConfig>();
            
            AddModule<ResourceMediaModule>();
            AddModule<RestModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Rest.Server";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.web.rest.server.quickstart";
        }
    }
        
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddRest(this ICompositeContainerBuilder composite, 
            Action<RestApiConfig> configure = null)
        {
            // Add plugin for Rest API support:
            composite.AddPlugin<RestPlugin>();

            // Call configure delegate on configuration if specified.
            if (configure != null)
            {
                composite.InitPluginConfig(configure);
            }
            
            return composite;
        }
    }
}