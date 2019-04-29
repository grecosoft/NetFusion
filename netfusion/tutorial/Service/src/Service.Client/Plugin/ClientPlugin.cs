using NetFusion.Bootstrap.Refactors;

namespace Service.Client.Plugin
{
    public class ClientPlugin : PluginDefinition
    {
        public override string PluginId => "fAD13D4BB-8777-4F73-8C6E-23BD03ABC433";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.HostPlugin;
        public override string Name => "Example Client Host.";
    }
}