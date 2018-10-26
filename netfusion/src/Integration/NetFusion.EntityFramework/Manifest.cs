using NetFusion.Bootstrap.Manifests;

namespace NetFusion.EntityFramework
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "9CADBB30-18EA-4CAF-A7EB-56540CD5E5C8";
        public string Name => "NetFusion EntityFramework Core Plugin.";
        public string Description => "Plugin providing bootstrapping and extensions to EntityFramework Core.";
    }
}
