using NetFusion.Bootstrap.Plugins;

namespace Service.WebApi.Plugin
{
    public class WebApiPlugin : PluginBase
    {
        public override string PluginId => "fddc1d2d-2f86-4d96-a1a8-e3de72c1a02a";
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string Name => "Example-WebApi";
    }
}