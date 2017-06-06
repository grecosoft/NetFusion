using NetFusion.Bootstrap.Manifests;

namespace NetFusion.RabbitMQ
{
    public class RabbitMQManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{086DBC42-CF80-4B1D-A4F2-C9DC7E3CA24C}";
        public string Name => "RabbitMQ Plug-in";

        public string Description => 
            "Provides support for integrating RabbitMQ as a message publisher.";
    }
}
