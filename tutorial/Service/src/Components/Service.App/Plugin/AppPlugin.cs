using NetFusion.Bootstrap.Plugins;
using Service.App.Plugin.Modules;

namespace Service.App.Plugin
{
    public class AppPlugin : PluginBase
    {
        public override string PluginId => "1623c246-2ed3-4606-8585-b90d57ff8d2a";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Application Services Component";

        public AppPlugin()
        {
            AddModule<ServiceModule>();
        }
    }
}