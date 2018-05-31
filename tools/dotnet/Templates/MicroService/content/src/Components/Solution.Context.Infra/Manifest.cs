using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.Infra
{
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:infra-id";
        public string Name => "Infrastructure Application Component";
        public string Description => "The plugin containing the application infrastructure.";
    }
}
