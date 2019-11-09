using NetFusion.Bootstrap.Plugins;
using Demo.Infra.Plugin.Modules;

namespace Demo.Infra.Plugin
{
    public class InfraPlugin : PluginBase
    {
        public override string PluginId => "b205e15d-2840-4966-92ab-f02dc93e0dbe";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Infrastructure Application Component";

        public InfraPlugin() {
            AddModule<RepositoryModule>();
            AddModule<AdapterModule>();
            Description = "Plugin component containing the application infrastructure.";
        }
    }
}
