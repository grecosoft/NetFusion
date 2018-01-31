using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Integration
{
    public class RabbitMqMongoDbManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "1E4D2D7F-75D5-486B-A3CB-1971EAD7C801"; 
        public string Name => "NetFusion RabbitMQ MongoDB Plug-in";

        public string Description => 
            "Plug-in that provides RabbitMQ MongoDB based implementations.";
    }
}
