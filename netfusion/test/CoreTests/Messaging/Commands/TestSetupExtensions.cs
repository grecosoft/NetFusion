using CoreTests.Messaging.Commands.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Plugin;
using NetFusion.Test.Plugins;

// ReSharper disable All

namespace CoreTests.Messaging.Commands
{
    public static class TestSetupExtensions
    {
        public static CompositeContainer WithHostCommandConsumer(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<MockCommandConsumer>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
        }

        public static CompositeContainer AddMultipleConsumers(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockInvalidCommandConsumer>();
            
            container.RegisterPlugins(appPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
        }
    }
}
