using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Rest.Server
{
    public class RestServerManifest : PluginManifestBase,
		ICorePluginManifest
	{
        public RestServerManifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Rest.Server";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.web.rest.server.quickstart";
        }

		public string PluginId => "77491AC3-31CC-44EC-B508-30E1ED2311CE";
		public string Name => "REST/HAL Server Implementation";
		public string Description => "Plug-in providing an implementation of REST-HAL.";
	}
}