using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Client
{
    public class HostManifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "{F54482C8-7D92-471E-B9AB-CF816D5EBC5A}";
        public string Name => "Example WebApi Host";
        public string Description => "Test WebApi host application with REST based routes.";
    }
}
