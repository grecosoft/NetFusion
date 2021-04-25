using CoreTests.Messaging.DomainEvents.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Plugin;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging.DomainEvents
{
    /// <summary>
    /// Provides composite-container configurations for testing publishing of domain-events.
    /// </summary>
    public static class TestSetupExtensions
    {
        public static CompositeContainer WithSyncDomainEventHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockSyncDomainEventConsumerOne>();
            container.RegisterPlugins(appPlugin);
            return container;
        }

        public static CompositeContainer WithAsyncDomainEventHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockAsyncDomainEventConsumerOne>();
            container.RegisterPlugins(appPlugin);
            return container;
        }

        public static CompositeContainer WithMultipleDomainEventHandlers(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockSyncDomainEventConsumerOne>();
            appPlugin.AddPluginType<MockAsyncDomainEventConsumerOne>();
            appPlugin.AddPluginType<MockSyncDomainEventConsumerTwo>();
            appPlugin.AddPluginType<MockAsyncDomainEventConsumerTwo>();
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        public static CompositeContainer WithDerivedDomainEventHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockDerivedMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        public static CompositeContainer WithDomainEventRuleHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockDomainEvenConsumerWithRule>();
            appPlugin.AddPluginType<MockRoleMin>();
            appPlugin.AddPluginType<MockRoleMax>();
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        
        
        
        
        
        
    
        
        // Adds application plugin containing a message handler throwing an exception.
        public static CompositeContainer WithMessageHandlerException(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockErrorMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        // Adds application plugin containing two message handlers.  The first domain-event handler
        // component injects IMessagingService and publishes a child domain-event resulting in an exception.
        public static CompositeContainer WithChildMessageHandlerException(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockErrorParentMessageConsumer>();
            appPlugin.AddPluginType<MockErrorChildMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        public static CompositeContainer WithHostAsyncConsumer(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<MockSyncDomainEventConsumerOne>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();

            return container;
        }
        
        
    }
}