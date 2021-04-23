using CoreTests.Messaging.DomainEvents.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Plugin;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging.DomainEvents
{
    /// <summary>
    /// Provides a basic setups for testing publishing of domain-events.
    /// </summary>
    public static class TestSetupExtensions
    {
        // Adds a domain-event handler component to an application plugin.
        public static CompositeContainer WithDomainEventHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockDomainEventConsumer>();
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        // Adds a domain-event handler component to an application plugin called for
        // a sent of domain-events derived from a common base domain-event.
        public static CompositeContainer WithDerivedDomainEventHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockDerivedMessageConsumer>();
            
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
            hostPlugin.AddPluginType<MockAsyncMessageConsumer>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();

            return container;
        }
        
        public static CompositeContainer WithDomainEventRuleHandler(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockDomainEventRuleBasedConsumer>();
            appPlugin.AddPluginType<MockRoleMin>();
            appPlugin.AddPluginType<MockRoleMax>();
            container.RegisterPlugins(appPlugin);
            return container;
        }
    }
}