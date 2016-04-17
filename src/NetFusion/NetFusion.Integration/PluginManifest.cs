using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Integration
{
    public class PluginManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId
        {
            get { return "55b93dae0a947288d20ff556"; }
        }

        public string Name
        {
            get { return "Integration Plug-in"; }
        }

        public string Description
        {
            get { return
                    "TODO"; }
        }
    }
}
