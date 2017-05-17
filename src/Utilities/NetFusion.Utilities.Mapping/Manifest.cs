using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Utilities.Mapping
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55bp3b9768949988d20ff995";
        public string Name => "Mapping Utility Plug-in";

        public string Description => 
            "Utility Plug-In used to map domain entities to other object representations.  " + 
            "This plug-in configures and coordinates the mapping process but does not dependent " +
            "on any specific mapping library.  The mapping library is specified by the host application.";
    }
}
