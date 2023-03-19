using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.UnitTests.Commands.Mocks;

namespace NetFusion.Messaging.UnitTests.Commands;

public static class TestSetupExtensions
{
    // ---------------- Command with Asynchronous Handlers ----------------
        
    public static CompositeContainer WithAsyncCommandConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
            
        appPlugin.AddPluginType<AsyncCommandRoute>();
        container.RegisterPlugins(appPlugin);

        return container;
    }

    public class AsyncCommandRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnCommand<MockCommand, MockCommandResult>(
                route => route.ToConsumer<MockAsyncCommandConsumer>(c => c.OnCommand));
                
            OnCommand<MockCommandNoResult>(
                route => route.ToConsumer<MockAsyncCommandConsumer>(c => c.OnCommandNoResult));
        }
    }
        
        
    // ---------------- Command with Synchronous Handler ----------------

    public static CompositeContainer WithSyncCommandConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
            
        appPlugin.AddPluginType<SyncCommandRoute>();
        container.RegisterPlugins(appPlugin);

        return container;
    }
        
    public class SyncCommandRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnCommand<MockCommand, MockCommandResult>(
                route => route.ToConsumer<MockSyncCommandConsumer>(c => c.OnCommand));
        }
    }
}