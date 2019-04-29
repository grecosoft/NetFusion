using NetFusion.Bootstrap.Refactors;

namespace Service.Infra.Plugin
{
    public class InfraPlugin : PluginDefinition
    {
        public override string PluginId => "6906fce4-0a69-423b-90ad-60547bfef835";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.ApplicationPlugin;
        public override string Name => "Plugin component containing the application infrastructure.";

        public InfraPlugin()
        {
            AddModule<RepositoryModule>();
        }
    }
}