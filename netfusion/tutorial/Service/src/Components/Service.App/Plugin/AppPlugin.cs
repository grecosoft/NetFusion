using NetFusion.Bootstrap.Refactors;

namespace Service.App.Plugin
{
    public class AppPlugin : PluginDefinition
    {
        public override string PluginId => "1623c246-2ed3-4606-8585-b90d57ff8d2a";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.ApplicationPlugin;
        public override string Name => "Application Services Component";
    }
}