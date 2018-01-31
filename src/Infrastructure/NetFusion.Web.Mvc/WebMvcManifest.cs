using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Web.Mvc
{
    public class WebMvcManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "3C757DE1-48E1-452D-959A-01C8961B43D8";
        public string Name => "Web MVC Plug-in";

        public string Description =>
            "Provides MVC related features that can be used to extend an MVC application.";
    }
}