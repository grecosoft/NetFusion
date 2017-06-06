using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Settings
{
    public class SettingsManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{1FC4C728-83E0-4407-B846-2871B3F0A1B6}";
        public string Name => "Settings Plug-in";

        public string Description =>
            "Plug-in that locates application settings and initializes them " +
            "when injected into a dependent component for the first time.";
    }
}
