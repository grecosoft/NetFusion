using NetFusion.Bootstrap.Manifests;

namespace Service.Infra
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins
    
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "6906fce4-0a69-423b-90ad-60547bfef835";
        public string Name => "Infrastructure Application Component";
        public string Description => "Plugin component containing the application infrastructure.";
    }
}
