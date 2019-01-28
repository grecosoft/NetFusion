using NetFusion.Bootstrap.Manifests;

namespace Service.App
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "1623c246-2ed3-4606-8585-b90d57ff8d2a";
        public string Name => "Application Services Component";
        public string Description => "Plugin component containing the Microservice's application services.";
    }
}
