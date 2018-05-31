using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.App
{
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:app-id";
        public string Name => "Application Services Component";
        public string Description => "The plugin containing the Microservice's application services.";
    }
}
