using NetFusion.Bootstrap.Manifests;

namespace NetFusion.RabbitMQ
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public Manifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.RabbitMq";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.overview#rabbitmq-overview";
        }
        
        public string PluginId => "2B389655-E790-4D30-B19C-C06AB8096C6A";
        public string Name => "RabbitMq Plugin based on EasyNetQ library.";
        public string Description => "Plugin for sending commands and publishing domain-events to RabbitMq";
    }
}
