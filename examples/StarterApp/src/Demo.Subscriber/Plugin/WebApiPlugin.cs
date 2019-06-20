using NetFusion.Bootstrap.Plugins;

namespace Demo.Subscriber.Plugin
{
    public class WebApiPlugin : PluginBase
    {
        public override string PluginId => "AD13D4BB-87E4-4F73-8C6E-23BD03ABC433";
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string Name => "WebApi REST Host";

        public WebApiPlugin()
        {
            
            Description = "WebApi host exposing REST/HAL based Web API.";
        }
    }
}
