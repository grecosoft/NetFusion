using NetFusion.Bootstrap.Manifests;

namespace Solution.Context.Api
{
    // This class identifies the assembly as a plug-in that will be discovered
    // by the bootstrap process.
    public class Manifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "nf:api-id";
        public string Name => "Microservice API Component";
        public string Description => "The plugin containing the Microservice's public API.";
    }
}
