using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Domain.Roslyn
{
    public class DomainEntityRoslynManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "5796a1c2d615e06365a68b0f";
        public string Name => "NetFusion Domain Roslyn Plug-in";

        public string Description =>
            "Plug-in that provides domain entity Roslyn based implementations.  This includes the runtime execution " +
            "of expressions against domain entities and it set of optional dynamic attributes.";
    }
}
