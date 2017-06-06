using NetFusion.Bootstrap.Manifests;

namespace NetFusion.MongoDB
{
    public class LoggingManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{1CC7754B-2847-4EED-9D8B-D414F9AE03FC}";
        public string Name => "NetFusion Logging Plug-in";

        public string Description =>
            "Plug-in providing base logging classes that do not pertain to any open-source logging libraries.";
    }
}
