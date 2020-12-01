using NetFusion.Bootstrap.Plugins;

namespace Subscriber.WebApi.Plugin
{
    public class WebApiPlugin : PluginBase
    {
        public const string HostId = "fAD13D4BB-8777-4F73-8C6E-23BD03ABC433";
        public const string HostName = "Example-Subscriber";
        
        public override PluginTypes PluginType => PluginTypes.HostPlugin;
        public override string PluginId => HostId;
        public override string Name => HostName;
    }
}