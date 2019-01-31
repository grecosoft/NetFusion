    using NetFusion.Bootstrap.Manifests;

namespace NetFusion.AMQP
{
    // https://github.com/grecosoft/NetFusion/wiki/core.bootstrap.plugins#bootstrapping---plugins
    public class Manifest : PluginManifestBase,
        ICorePluginManifest
    {
        public Manifest()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.Azure.Messaging";
        }
        
        public string PluginId => "35273B60-72EE-4428-97F1-2EB51A88B32A";
        public string Name => "NetFusion Azure Messaging.";
        public string Description => "Plugin containing Azure messaging integrations using AMQP.";
    }
}
