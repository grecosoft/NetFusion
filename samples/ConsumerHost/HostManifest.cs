using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Client
{
    public class HostManifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "{0738190E-DF28-4819-8303-1AC64279A447}";
        public string Name => "Example Message Consumer Host";
        public string Description => "Test message consumer host.";
    }
}
