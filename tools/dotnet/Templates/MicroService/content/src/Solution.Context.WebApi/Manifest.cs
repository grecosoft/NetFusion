using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.WebApi
{
    public class HostManifest : PluginManifestBase,
         IAppHostPluginManifest
    {
        public string PluginId => "nf:host-id";
        public string Name => "WebApi REST Host";
        public string Description => "WebApi host exposing REST/HAL based API.";
    }
}
