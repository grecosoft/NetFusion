using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.UnitTests.Commands.Mocks;

// ReSharper disable All

namespace CoreTests.Messaging.Commands;

public static class TestSetupExtensions
{
    public static CompositeContainer WithAsyncCommandConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
            
        appPlugin.AddPluginType<MockAsyncCommandConsumer>();
        container.RegisterPlugins(appPlugin);

        return container;
    }

    public static CompositeContainer WithSyncCommandConsumer(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
            
        appPlugin.AddPluginType<MockSyncCommandConsumer>();
        container.RegisterPlugins(appPlugin);

        return container;
    }

    public static CompositeContainer WithMultipleConsumers(this CompositeContainer container)
    {
        var appPlugin1 = new MockAppPlugin();
        appPlugin1.AddPluginType<MockAsyncCommandConsumer>();
            
        var appPlugin2 = new MockAppPlugin();
        appPlugin2.AddPluginType<MockInvalidCommandConsumer>();
            
        container.RegisterPlugins(appPlugin1, appPlugin2);

        return container;
    }
}