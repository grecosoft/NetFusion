using CoreTests.Messaging.Commands.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;

// ReSharper disable All

namespace CoreTests.Messaging.Commands
{
    public static class TestSetupExtensions
    {
        public static CompositeContainer WithCommandConsumer(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            
            appPlugin.AddPluginType<MockCommandConsumer>();
            container.RegisterPlugins(appPlugin);

            return container;
        }

        public static CompositeContainer WithMultipleConsumers(this CompositeContainer container)
        {
            var appPlugin1 = new MockAppPlugin();
            appPlugin1.AddPluginType<MockCommandConsumer>();
            
            var appPlugin2 = new MockAppPlugin();
            appPlugin2.AddPluginType<MockInvalidCommandConsumer>();
            
            container.RegisterPlugins(appPlugin1, appPlugin2);

            return container;
        }
    }
}
