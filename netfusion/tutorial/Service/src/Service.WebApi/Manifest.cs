using NetFusion.Bootstrap.Manifests;

namespace Service.WebApi
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class HostManifest : PluginManifestBase,
         IAppHostPluginManifest
    {
        public string PluginId => "fddc1d2d-2f86-4d96-a1a8-e3de72c1a02a";
        public string Name => "WebApi REST Host";
        public string Description => "WebApi host exposing REST/HAL based Web API.";
    }
}
