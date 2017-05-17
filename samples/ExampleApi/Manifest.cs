using NetFusion.Bootstrap.Manifests;

namespace RefArch.Api
{

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "576b2a9a000a7ed1176f1772";
        public string Name => "Public Application Programming Interface.";
        public string Description => "Provides commands and responses for business use cases.";
    }
   
}
