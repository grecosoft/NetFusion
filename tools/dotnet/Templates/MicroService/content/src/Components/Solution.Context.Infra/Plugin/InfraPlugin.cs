using NetFusion.Bootstrap.Plugins;
using Solution.Context.Infra.Repositories;

namespace Solution.Context.Infra.Plugin
{
    public class InfraPlugin : PluginBase
    {
        public override string PluginId => "nf:infra-id";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Infrastructure Application Component";

        public InfraPlugin() {
            AddModule<RepositoryModule>();

            Description = "Plugin component containing the application infrastructure.";
        }
    }
}
