using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Messaging
{
    public class PluginManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93b1a0a947288d20ff225";
        public string Name => "Messaging Plug-in"; 

        public string Description =>
             "Contains common implementation for publishing messages " + 
             "that can be extended by other plug-ins."; 
    }
}
