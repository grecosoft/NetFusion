using System;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Rest.Server.Plugin
{
    public class RestPlugin : Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "77491AC3-31CC-44EC-B508-30E1ED2311CE";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "REST/HAL Server Implementation";

        public RestPlugin()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Rest.Server";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.web.rest.server.quickstart";
            
            AddConfig<RestApiConfig>();
            
            AddModule<ResourceMediaModule>();
            AddModule<RestModule>();
        }
    }
        
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddRest(this IComposeAppBuilder composite, 
            Action<RestApiConfig> configure = null)
        {
            composite.AddPlugin<RestPlugin>();

            if (configure != null)
            {
                RestApiConfig config = composite.GetConfig<RestApiConfig>();
                configure(config);
            }
            
            return composite;
        }
    }
}