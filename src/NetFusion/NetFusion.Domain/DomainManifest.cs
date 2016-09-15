using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Domain
{ 
    public class DomainManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55bp3b1a0a947288d20ff125";
        public string Name => "Domain Plug-in";

        public string Description =>
            "Provides patterns that can be used to extend domain entities.  " +
            "Also provides other domain based patterns.";
    }
}
