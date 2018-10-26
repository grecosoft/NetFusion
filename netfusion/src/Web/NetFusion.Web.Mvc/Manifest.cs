using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Web.Mvc
{
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public Manifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion-Plugins/tree/master/src/Infrastructure/NetFusion.Web.Mvc";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/infrastructure.web-mvc.overview";
        }

        public string PluginId => "3C757DE1-48E1-452D-959A-01C8961B43D8";
        public string Name => "Web MVC Plug-in";

        public string Description =>
            "Provides MVC related features that can be used to extend an MVC application.";
    }
}