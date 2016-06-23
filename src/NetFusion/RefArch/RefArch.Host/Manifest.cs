using NetFusion.Bootstrap.Manifests;

namespace Samples.WebHost
{
    public class Manifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "576b2a97000a7ed1176f1771";
        public string Name => "Sample Web Host";
        public string Description => "Example host providing examples.";
    }
}