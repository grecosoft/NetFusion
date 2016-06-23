using NetFusion.Bootstrap.Manifests;

namespace Samples.WebHost
{
    public class Manifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "576b2a9d000a7ed1176f1778";
        public string Name => "Sample Subscription Client";
        public string Description => "Example host providing examples for consuming message bus events.";
    }
}