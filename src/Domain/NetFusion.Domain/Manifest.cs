using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Domain
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{CB97D67FG-8455-4866-B839-A66A062E2870}";
        public string Name => "Domain Plug-in";

        public string Description =>
            "Provides patterns that can be used to extend domain entities.  " +
            "Also provides other domain based patterns.";
    }
}
