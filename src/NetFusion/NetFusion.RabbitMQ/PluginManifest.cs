using NetFusion.Bootstrap.Manifests;

namespace NetFusion.RabbitMQ
{
    public class PluginManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId
        {
            get { return "55b93dae0a947288d20ff236"; }
        }

        public string Name
        {
            get { return "RabbitMQ Plug-in"; }
        }

        public string Description
        {
            get { return
                    "Provides support for integrating event publishers " +
                    "and consumers."; }
        }
    }
}
