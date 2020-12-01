using NetFusion.Bootstrap.Plugins;

namespace Service.WebApi.Plugin
{
    public class WebApiPlugin : PluginBase
    {
        public const string HostId = "fddc1d2d-2f86-4d96-a1a8-e3de72c1a02a";
        public const string HostName = "Example-WebApi";

        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string PluginId => HostId;
        public override string Name => HostName;
    }
}