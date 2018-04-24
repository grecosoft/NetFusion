using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Rest.Config
{
    public class RestConfigManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public RestConfigManifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Rest.Config";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.web.rest.client.quickstart";
        }

        public string PluginId => "0B60AEE1-1365-4A64-AB62-D4D6A9B6AB32";
        public string Name => "REST Client Configuration";
        public string Description => "Plug-in that will configure the Request Client Factory based on application settings.";
    }
}