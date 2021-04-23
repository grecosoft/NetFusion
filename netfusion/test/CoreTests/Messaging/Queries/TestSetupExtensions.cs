using CoreTests.Messaging.Queries.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging.Queries
{
    public static class TestSetupExtensions
    {
        public static CompositeContainer WithQueryConsumer(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockQueryConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        public static CompositeContainer WithMultipleQueryConsumers(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<DuplicateConsumerOne>();
            appPlugin.AddPluginType<DuplicateConsumerTwo>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
    }
}