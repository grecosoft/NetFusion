using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Client
{
    public class HostManifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "55b93b110a947288d20ff2992b";
        public string Name => "Example WebApi Host";
        public string Description => "Test WebApi host application with REST based routes.";
    }
}
