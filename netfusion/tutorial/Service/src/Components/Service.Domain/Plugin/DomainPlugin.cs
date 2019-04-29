using NetFusion.Bootstrap.Refactors;

namespace Service.Domain.Plugin
{
    public class DomainPlugin : PluginDefinition
    {
        public override string PluginId => "77937237-e70d-492d-9084-d5e5570c8d05";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.ApplicationPlugin;
        public override string Name => "Domain Model Component";
    }
}