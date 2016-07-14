using NetFusion.Bootstrap.Manifests;

namespace NetFusion.MongoDB
{
    public class LoggingManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "5786db1359303da296481377";
        public string Name => "NetFusion Logging Plug-in";

        public string Description =>
            "Plug-in providing base logging classes that do not pertain to any open-source logging libraries.";
    }
}
