using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.Infra
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins
    
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:infra-id";
        public string Name => "Infrastructure Application Component";
        public string Description => "Plugin component containing the application infrastructure.";
    }
}
