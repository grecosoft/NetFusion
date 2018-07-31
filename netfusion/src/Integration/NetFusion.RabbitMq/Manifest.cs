using NetFusion.Bootstrap.Manifests;

namespace NetFusion.RabbitMQ
{
    public class RabbitMqManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "2B389655-E790-4D30-B19C-C06AB8096C6A";
        public string Name => "RabbitMq Plugin based on EasyNetQ library.";
        public string Description => "Plugin for sending commands and publishing domain-events to RabbitMq";
    }
}
