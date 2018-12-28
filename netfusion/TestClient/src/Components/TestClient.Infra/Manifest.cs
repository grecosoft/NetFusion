using NetFusion.Bootstrap.Manifests;

namespace TestClient.Infra
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins
    
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "03b936c5-1ec1-492a-b16a-ebfdfc13f941";
        public string Name => "Infrastructure Application Component";
        public string Description => "Plugin component containing the application infrastructure.";
    }
}
