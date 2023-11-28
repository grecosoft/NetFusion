using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.UnitTests.DomainEvents.Mocks;

namespace NetFusion.Messaging.UnitTests.DomainEvents;

/// <summary>
/// Provides composite-container configurations for testing publishing of domain-events.
/// </summary>
public static class TestSetupExtensions
{
    public static ICompositeContainer WithSyncDomainEventHandler(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockSyncDomainEventConsumerOne>();
        container.RegisterPlugins(appPlugin);
        return container;
    }

    public static ICompositeContainer WithAsyncDomainEventHandler(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockAsyncDomainEventConsumerOne>();
        container.RegisterPlugins(appPlugin);
        return container;
    }

    public static ICompositeContainer WithMultipleDomainEventHandlers(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockSyncDomainEventConsumerOne>();
        appPlugin.AddPluginType<MockAsyncDomainEventConsumerOne>();
        appPlugin.AddPluginType<MockSyncDomainEventConsumerTwo>();
        appPlugin.AddPluginType<MockAsyncDomainEventConsumerTwo>();
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public static ICompositeContainer WithDerivedDomainEventHandler(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockDerivedMessageConsumer>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    // Adds application plugin containing a message handler throwing an exception.
    public static ICompositeContainer WithMessageHandlerException(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockErrorMessageConsumer>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    // Adds application plugin containing two message handlers.  The first domain-event handler
    // component injects IMessagingService and publishes a child domain-event resulting in an exception.
    public static ICompositeContainer WithChildMessageHandlerException(this ICompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MockErrorParentMessageConsumer>();
        appPlugin.AddPluginType<MockErrorChildMessageConsumer>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public static ICompositeContainer WithHostAsyncConsumer(this ICompositeContainer container)
    {
        var hostPlugin = new MockHostPlugin();
        hostPlugin.AddPluginType<MockSyncDomainEventConsumerOne>();
            
        container.RegisterPlugins(hostPlugin);
        container.RegisterPlugin<MessagingPlugin>();

        return container;
    }
        
        
}