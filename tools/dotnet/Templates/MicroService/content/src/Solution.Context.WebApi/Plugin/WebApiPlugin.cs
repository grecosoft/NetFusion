using NetFusion.Bootstrap.Plugins;

namespace Solution.Context.WebApi.Plugin
{
    public class WebApiPlugin : PluginBase
    {
        public override string PluginId => "nf:host-id";
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string Name => "WebApi REST Host";

        public WebApiPlugin()
        {
            Description = "WebApi host exposing REST/HAL based Web API.";
        }
    }
}