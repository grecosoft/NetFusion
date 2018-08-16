
using NetFusion.Bootstrap.Manifests;

namespace Demo.WebApi
{
    public class HostManifest : PluginManifestBase,
        IAppHostPluginManifest
    {
        public string PluginId => "AD13D4BB-87E4-4F73-8C6E-23BD03ABC433";
        public string Name => "Example WebApi Host";
        public string Description => "Test WebApi host application.";
    }
}
