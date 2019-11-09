using NetFusion.Bootstrap.Plugins;

namespace Demo.WebApi.Plugin
{
    public class WebApiPlugin : PluginBase
    {
        public override string PluginId => "6d9596da-cd07-4467-924b-398e0d83e4be";
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string Name => "WebApi REST Host";

        public WebApiPlugin()
        {
            Description = "WebApi host exposing REST/HAL based Web API.";
        }
    }
}