using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Messaging
{
    public class MessagingManifest : PluginManifestBase,
        ICorePluginManifest
    {
        public string PluginId => "{4576D809-E216-4C03-BE43-737728047BAA}";
        public string Name => "Messaging Plug-in"; 

        public string Description =>
             "Contains common implementation for publishing messages in-process that " +
             "can be extended by other plug-ins to publish messages out of process."; 
    }
}
