using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Rest.Config.Modules
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "0B60AEE1-1365-4A64-AB62-D4D6A9B6AB32";
        public string Name => "REST Client Configuration";
        public string Description => "Plug-in that will configure the Request Client Factory based on application settings.";
    }
}