using NetFusion.Bootstrap.Manifests;

namespace NetFusion.WebApi
{
    public class WebApiManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId
        {
            get { return "55b93dac0a947288d20ff232"; }
        }

        public string Name
        {
            get { return "WebApi Plug-in"; }
        }

        public string Description
        {
            get
            {
                return
                    "Provides easy integration with Autofac, JWT Security, and other WebApi services and utilities.";
            }
        }
    }
}