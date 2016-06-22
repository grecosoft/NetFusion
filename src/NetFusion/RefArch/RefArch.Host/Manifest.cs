using NetFusion.Bootstrap.Manifests;

namespace Samples.WebHost
{
    public class Manifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "56b68f27a694e92f6ca03ee6";
        public string Name => "Sample Web Host";
        public string Description => "Example host providing examples.";
    }
}