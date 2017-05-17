using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Client
{
    public class HostManifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "55b93b110a947288d20ff2991b";
        public string Name => "Example Message Consumer Host";
        public string Description => "Test message consumer host.";
    }
}
