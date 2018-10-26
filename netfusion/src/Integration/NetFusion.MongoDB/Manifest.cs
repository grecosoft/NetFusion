using NetFusion.Bootstrap.Manifests;

namespace NetFusion.MongoDB
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public Manifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.MongoDB";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.mongodb.overview";
        }

        public string PluginId => "4BE391F9-F687-4E49-90A3-D38300E3A751"; 
        public string Name => "MongoDb Plug-in";

        public string Description =>
            "Plug-in that registers a client for connecting to a MongoDB instance." +
            "Also finds all MongoDb entity type mappings and configures then with the client.";
    }
}
