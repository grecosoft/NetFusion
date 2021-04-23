using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Plugin;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging
{
    public static class TestSetupExtensions
    {
        // Adds a host plugin configured with the core messaging plugin.
        public static CompositeContainer AddMessagingHost(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
        }
    }
}