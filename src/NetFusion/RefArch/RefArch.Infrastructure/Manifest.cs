using NetFusion.Bootstrap.Manifests;

namespace RefArch.Infrastructure
{
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "576b2a9b000a7ed1176f1774";
        public string Name => "Infrastructure Component";
        public string Description => "Provides infrastructure to support the business domain.";
    }
}
