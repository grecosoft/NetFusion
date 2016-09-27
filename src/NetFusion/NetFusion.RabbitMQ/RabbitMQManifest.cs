using NetFusion.Bootstrap.Manifests;

namespace NetFusion.RabbitMQ
{
    public class RabbitMQManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93dae0a947288d20ff236";
        public string Name => "RabbitMQ Plug-in";

        public string Description => 
            "Provides support for integrating RabbitMQ as a message publisher.";
    }
}
