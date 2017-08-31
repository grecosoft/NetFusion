using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Rest.Server.Modules
{
	public class Manifest : PluginManifestBase,
		ICorePluginManifest
	{
		public string PluginId => "{77491AC3-31CC-44EC-B508-30E1ED2311CE}";
		public string Name => "REST Server Implementation";
		public string Description => "Plug-in providing an implementation of REST-HAL.";
	}
}