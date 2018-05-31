using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.Domain
{
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:domain-id";
        public string Name => "Domain Model Component";
        public string Description => "The component containing the Microservice's domain model.";
    }

}
