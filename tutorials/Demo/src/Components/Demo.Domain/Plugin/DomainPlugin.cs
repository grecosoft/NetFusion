using NetFusion.Bootstrap.Plugins;

namespace Demo.Domain.Plugin
{
    public class DomainPlugin : PluginBase
    {
        public override string PluginId => "97eabf23-7e49-4b8f-b13e-d91e37ebdd23";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Domain Model Component";
        
        public DomainPlugin()
        {
            Description = "Plugin component containing the Microservice's domain model.";
        }
    }
}