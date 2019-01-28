using NetFusion.Bootstrap.Manifests;

namespace Service.Domain
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "77937237-e70d-492d-9084-d5e5570c8d05";
        public string Name => "Domain Model Component";
        public string Description => "Plugin component containing the Microservice's domain model.";
    }

}
