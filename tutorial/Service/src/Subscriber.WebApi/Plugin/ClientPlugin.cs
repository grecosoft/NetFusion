using NetFusion.Bootstrap.Plugins;

namespace Subscriber.WebApi.Plugin
{
    public class ClientPlugin : PluginBase
    {
        public override string PluginId => "fAD13D4BB-8777-4F73-8C6E-23BD03ABC433";
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string Name => "Example Client Host.";
    }
}