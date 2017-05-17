using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Settings
{
    public class SettingsManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93f3e0a947288d20ff238";
        public string Name => "Settings Plug-in";

        public string Description =>
            "Plug-in that locates application settings and initializes them " +
            "when injected into a dependent component for the first time.";
    }
}
