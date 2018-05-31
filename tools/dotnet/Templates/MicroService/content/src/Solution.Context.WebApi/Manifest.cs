using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.WebApi
{
    public class HostManifest : PluginManifestBase,
         IAppHostPluginManifest
    {
        public string PluginId => "nf:host-id";
        public string Name => "Contacts WebApi Host";
        public string Description => "WebApi host exposing Contacts REST/HAL based API.";
    }
}
