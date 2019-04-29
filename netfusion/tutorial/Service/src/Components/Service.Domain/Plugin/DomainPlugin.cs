using NetFusion.Bootstrap.Plugins;

namespace Service.Domain.Plugin
{
    public class DomainPlugin : PluginBase
    {
        public override string PluginId => "77937237-e70d-492d-9084-d5e5570c8d05";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Domain Model Component";
    }
}