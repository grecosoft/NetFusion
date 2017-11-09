using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Mapping
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{83C90E78-D245-4B0D-A4FC-E74B11227766}";
        public string Name => "Mapping Plug-in";

        public string Description => 
            "Plug-In used to map domain entities to other object representations.  This plug-in configures and " + 
            "coordinates the mapping process but does not dependent on any specific mapping library.  The host " +
            "application can use an open-source mapping library of choice.";
    }
}
