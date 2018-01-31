using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Domain.Roslyn
{
    public class DomainEntityRoslynManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "316A9C70-C3AE-4DC2-8DEA-097EBDB342F7";
        public string Name => "NetFusion Roslyn Plug-in";

        public string Description =>
            "Plug-in that provides domain entity Roslyn based implementations.  This includes the runtime execution " +
            "of expressions against domain entities and it set of optional dynamic attributes.";
    }
}
