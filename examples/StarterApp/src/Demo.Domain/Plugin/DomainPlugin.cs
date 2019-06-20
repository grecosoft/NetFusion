using NetFusion.Bootstrap.Plugins;

namespace Demo.Domain.Plugin
{
    public class DomainPlugin : PluginBase
    {
        public override string PluginId => "BD556571-695C-4F7E-B02F-811DA2ACB16C";
        public override PluginTypes PluginType => PluginTypes.ApplicationPlugin;
        public override string Name => "Domain Model Component";
        
        public DomainPlugin()
        {
            Description = "Plugin component containing the Microservice's domain model.";
        }
    }
}
