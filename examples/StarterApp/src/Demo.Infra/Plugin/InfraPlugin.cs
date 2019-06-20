using Demo.Infra.Plugin.Modules;
using NetFusion.Bootstrap.Plugins;

namespace Demo.Infra.Plugin
{
    public class InfraPlugin : PluginBase
    {
        public override string PluginId => "ABF99168-CFD0-4B7E-9704-4094C26FD019";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Infrastructure Application Component";

        public InfraPlugin() {
            AddModule<RepositoryModule>();
            AddModule<AdapterModule>();

            Description = "Plugin component containing the application infrastructure.";
        }
    }
}
