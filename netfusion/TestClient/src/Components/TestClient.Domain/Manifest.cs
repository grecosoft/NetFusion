using NetFusion.Bootstrap.Manifests;

namespace TestClient.Domain
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "4c326b7a-5e2d-4378-a4bf-d63a347ae9e6";
        public string Name => "Domain Model Component";
        public string Description => "Plugin component containing the Microservice's domain model.";
    }

}
