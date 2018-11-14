using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.Domain
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins

    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:domain-id";
        public string Name => "Domain Model Component";
        public string Description => "Plugin component containing the Microservice's domain model.";
    }

}
