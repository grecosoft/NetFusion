using NetFusion.Bootstrap.Manifests;

namespace NetFusion.MongoDB
{
    public class EntityFrameworkManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{F66BA457-F387-4D45-9972-31EC9DB7D4A0}"; 
        public string Name => "EntityFramework Plug-in";

        public string Description => "Provides initialization for DBContext and " + 
            "also allows for automatic registration of entity mappings.";
    }
}
