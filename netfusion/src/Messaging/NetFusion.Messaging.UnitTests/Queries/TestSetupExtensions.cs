using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.UnitTests.Queries.Mocks;

namespace NetFusion.Messaging.UnitTests.Queries;

public static class TestSetupExtensions
{
    public static CompositeContainer WithSyncQueryConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<SyncQueryConsumerRoute>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }

    private class SyncQueryConsumerRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnQuery<MockQuery, MockQueryResult>(
                route => route.ToConsumer<MockSyncQueryConsumer>(c => c.Execute));
        }
    }
        
    public static CompositeContainer WithAsyncQueryConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<AsyncSyncQueryConsumerRoute>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    private class AsyncSyncQueryConsumerRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnQuery<MockQuery, MockQueryResult>(
                route => route.ToConsumer<MockAsyncQueryConsumer>(c => c.Execute));
        }
    }
}