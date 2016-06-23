using NetFusion.Bootstrap.Manifests;

namespace RefArch.Services
{
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "576b2a9c000a7ed1176f1775";
        public string Name => "Application Services";
        public string Description => "Provides implementation of application level services.";
    }
}
