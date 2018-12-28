using NetFusion.Bootstrap.Manifests;

namespace TestClient.App
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "d473313c-2408-4637-8ce4-2b4a8f179e69";
        public string Name => "Application Services Component";
        public string Description => "Plugin component containing the Microservice's application services.";
    }
}
