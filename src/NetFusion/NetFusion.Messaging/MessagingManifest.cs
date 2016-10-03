using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Messaging
{ 
    public class MessagingManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "55b93b1a0a947288d20ff225";
        public string Name => "Messaging Plug-in"; 

        public string Description =>
             "Contains common implementation for publishing messages in-process that " +
             "can be extended by other plug-ins to publish messages out of process."; 
    }
}
