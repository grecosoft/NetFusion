using NetFusion.Bootstrap.Manifests;

namespace RefArch.Api
{

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "E797B872-DDC9-418C-9DED-DF981BA164FC";
        public string Name => "Public Application Programming Interface.";
        public string Description => "Provides commands and responses for business use cases.";
    }
   
}
