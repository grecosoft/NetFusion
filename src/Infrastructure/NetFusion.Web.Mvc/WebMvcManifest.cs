using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Web.Mvc
{
    public class WebMvcManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93ea00a947288d20ff299";
        public string Name => "Web MVC Plug-in";

        public string Description =>
            "Provides MVC related features that can be used to extend an MVC application.";
    }
}