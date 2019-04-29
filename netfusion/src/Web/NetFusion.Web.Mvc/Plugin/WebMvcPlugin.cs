using System;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Web.Mvc.Metadata.Modules;

namespace NetFusion.Web.Mvc.Plugin
{
    public class WebMvcPlugin : Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "3C757DE1-48E1-452D-959A-01C8961B43D8";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "Web MVC Plug-in";
        
        public WebMvcPlugin()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Web.Mvc";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.web-mvc.overview";
            
            AddConfig<WebMvcConfig>();
            AddModule<ApiMetadataModule>();
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddWebMvc(this IComposeAppBuilder composite, Action<WebMvcConfig> configure = null)
        {
            composite.AddPlugin<WebMvcPlugin>();

            
            if (configure != null)
            {
                var config = composite.GetConfig<WebMvcConfig>();
                configure(config);
            }
            
            return composite;
        }
    }
}