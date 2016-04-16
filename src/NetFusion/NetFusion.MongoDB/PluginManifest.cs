using NetFusion.Bootstrap.Manifests;

namespace NetFusion.MongoDB
{
    public class PluginManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93ea00a947288d20ff237"; 
        public string Name => "NetFusion MongoDb Plug-in";

        public string Description =>
            "Plug-in that registers a client for connecting to a MongoDB instance." +
            "Also finds all MongoDb entity type mappings and configures then with the client.";
    }
}
