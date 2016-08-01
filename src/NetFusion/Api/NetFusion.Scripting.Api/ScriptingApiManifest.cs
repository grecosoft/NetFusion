using NetFusion.Bootstrap.Manifests;

namespace NetFusion.Messaging
{
    public class MessagingManifest : PluginManifestBase,
        IAppComponentPluginManifest
    {
        public string PluginId => "579900d2f52be78d7f027d9a";
        public string Name => "NetFusion Scripting Api";

        public string Description =>
             "Provides an API for maintaining entity script metadata.";
    }
}
