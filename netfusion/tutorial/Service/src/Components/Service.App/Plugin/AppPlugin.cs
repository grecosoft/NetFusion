using NetFusion.Bootstrap.Plugins;

namespace Service.App.Plugin
{
    public class AppPlugin : NetFusion.Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "1623c246-2ed3-4606-8585-b90d57ff8d2a";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Application Services Component";
    }
}