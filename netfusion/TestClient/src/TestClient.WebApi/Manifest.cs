using NetFusion.Bootstrap.Manifests;

namespace TestClient.WebApi
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class HostManifest : PluginManifestBase,
         IAppHostPluginManifest
    {
        public string PluginId => "e7f0e13e-fe8c-412d-afec-0e586464cb42";
        public string Name => "WebApi REST Host";
        public string Description => "WebApi host exposing REST/HAL based Web API.";
    }
}
