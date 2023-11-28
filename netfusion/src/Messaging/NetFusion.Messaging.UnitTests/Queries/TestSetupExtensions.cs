using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

namespace NetFusion.Messaging.UnitTests.Queries;

public static class TestSetupExtensions
{
    public static ICompositeContainer WithSyncQueryConsumer(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockSyncQueryConsumer>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public static ICompositeContainer WithAsyncQueryConsumer(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockAsyncQueryConsumer>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public static ICompositeContainer WithMultipleQueryConsumers(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<DuplicateConsumerOne>();
        appPlugin.AddPluginType<DuplicateConsumerTwo>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
}