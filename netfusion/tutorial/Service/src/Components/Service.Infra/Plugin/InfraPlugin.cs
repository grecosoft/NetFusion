using NetFusion.Bootstrap.Plugins;

namespace Service.Infra.Plugin
{
    public class InfraPlugin : NetFusion.Bootstrap.Plugins.Plugin
    {
        public override string PluginId => "6906fce4-0a69-423b-90ad-60547bfef835";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Plugin component containing the application infrastructure.";

        public InfraPlugin()
        {
            AddModule<RepositoryModule>();
        }
    }
}