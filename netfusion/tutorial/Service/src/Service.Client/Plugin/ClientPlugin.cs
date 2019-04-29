using NetFusion.Bootstrap.Plugins;

namespace Service.Client.Plugin
{
    public class ClientPlugin : NetFusion.Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "fAD13D4BB-8777-4F73-8C6E-23BD03ABC433";
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string Name => "Example Client Host.";
    }
}