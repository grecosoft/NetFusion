using NetFusion.Bootstrap.Manifests;

namespace NetFusion.MongoDB
{
    public class EntityFrameworkManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b944a00a947288d20ff237"; 
        public string Name => "NetFusion EntityFramework Plug-in";

        public string Description => "Provides initialization for DBContext and " + 
            "also allows for automatic registration of entity mappings.";
    }
}
