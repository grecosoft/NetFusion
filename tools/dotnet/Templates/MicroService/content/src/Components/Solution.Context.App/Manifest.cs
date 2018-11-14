using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.App
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:app-id";
        public string Name => "Application Services Component";
        public string Description => "Plugin component containing the Microservice's application services.";
    }
}
