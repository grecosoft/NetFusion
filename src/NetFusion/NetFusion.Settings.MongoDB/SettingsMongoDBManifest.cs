using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Settings.MongoDB
{
    public class SettingsMongoDBManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93f3e0a947288d997f238";
        public string Name => "NetFusion MongoDb Application Settings Plug-in";
        public string Description => 
            "Application settings initializer that loads application settings from MongoDB.";
    }
}
