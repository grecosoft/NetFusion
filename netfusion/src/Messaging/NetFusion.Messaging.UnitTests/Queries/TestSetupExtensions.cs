using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

namespace NetFusion.Messaging.UnitTests.Queries;

public static class TestSetupExtensions
{
    public static CompositeContainer WithSyncQueryConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockSyncQueryConsumer>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public static CompositeContainer WithAsyncQueryConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockAsyncQueryConsumer>();
            
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