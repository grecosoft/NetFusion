using NetFusion.Core.Bootstrap.Plugins;

namespace Examples.nfTopic.Domain.Plugin;

public class DomainPlugin : PluginBase
{
    public override string PluginId => "[nf:domain-id]";
    public override PluginTypes PluginType => PluginTypes.AppPlugin;
    public override string Name => "Domain Model Component";
        
    public DomainPlugin()
    {
        Description = "Plugin component containing the Microservice's domain model.";
    }
}