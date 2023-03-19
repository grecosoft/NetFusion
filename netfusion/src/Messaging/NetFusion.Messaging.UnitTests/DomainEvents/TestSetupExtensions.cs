using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.UnitTests.DomainEvents.Mocks;

namespace NetFusion.Messaging.UnitTests.DomainEvents;

/// <summary>
/// Provides composite-container configurations for testing publishing of domain-events.
/// </summary>
public static class TestSetupExtensions
{
    // ---------------- Domain-Event with Synchronous Handler ----------------
        
    public static CompositeContainer WithSyncDomainEventHandler(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<SingleSyncDomainEventRoute>();
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class SingleSyncDomainEventRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockSyncDomainEventConsumerOne>(
                    c => c.OnEventHandler));
        }
    }
        
        
    // ---------------- Domain-Event with Asynchronous Handler ----------------
        
    public static CompositeContainer WithAsyncDomainEventHandler(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<SingleAsyncDomainEventRoute>();
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class SingleAsyncDomainEventRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockAsyncDomainEventConsumerOne>(
                    c => c.OnEventHandler));
        }
    }
        
        
    // ---------------- Domain-Event with Derived Event Handler ----------------
        
    public static CompositeContainer WithDerivedDomainEventHandler(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<DerivedDomainEventsRoute>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class DerivedDomainEventsRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockBaseDomainEvent>(
                route => route.ToConsumer<MockDerivedMessageConsumer>(
                    c => c.OnBaseEventHandler, 
                    meta => meta.IncludedDerivedMessages = true));
        }
    }
        
    // ---------------- Domain-Event with Multiple Synchronous/Asynchronous Handlers  ----------------
        
    public static CompositeContainer WithMultipleDomainEventHandlers(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<MultipleDomainEventHandler>();
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class MultipleDomainEventHandler : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockSyncDomainEventConsumerOne>(
                    c => c.OnEventHandler));
                
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockAsyncDomainEventConsumerOne>(
                    c => c.OnEventHandler));
                
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockSyncDomainEventConsumerTwo>(
                    c => c.OnEventHandler));
                
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockAsyncDomainEventConsumerTwo>(
                    c => c.OnEventHandler));
        }
    }
        
        
    // ---------------- Domain-Event with Handler with Predicate  ----------------
        
    public static CompositeContainer WithDomainEventPredicateHandler(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<PredicateDomainEventRoute>();
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class PredicateDomainEventRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockRuleDomainEvent>(
                route => route.ToConsumer<MockDomainEvenConsumerWithRule>(
                    c => c.OnEventAllRulesPass, 
                    meta => meta.When(e => 1000 <= e.RuleTestValue && e.RuleTestValue <= 2000)));
                
            OnDomainEvent<MockRuleDomainEvent>(route => route.ToConsumer<MockDomainEvenConsumerWithRule>(
                c => c.OnEventAnyRulePasses, 
                meta => meta.When(e => e.RuleTestValue is >= 1000 and <= 2000)));
        }
    }

    // ---------------- Domain-Event with Handler with Exceptions  ----------------
        
    // Adds application plugin containing a message handler throwing an exception.
    public static CompositeContainer WithMessageHandlerException(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<HandlerExceptionEventRoute>();
            
        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class HandlerExceptionEventRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockErrorMessageConsumer>(
                    c => c.OnEventAsync));
        }
    }
        
    // Adds application plugin containing two message handlers.  The first domain-event handler
    // component injects IMessagingService and publishes a child domain-event resulting in an exception.
    public static CompositeContainer WithChildMessageHandlerException(this CompositeContainer container)
    {
        var appPlugin = new MockAppPlugin();
        appPlugin.AddPluginType<ChildHandlerExceptionEventRoute>();

        container.RegisterPlugins(appPlugin);
        return container;
    }
        
    public class ChildHandlerExceptionEventRoute : MessageRouter
    {
        protected override void OnConfigureRoutes()
        {
            OnDomainEvent<MockDomainEvent>(
                route => route.ToConsumer<MockErrorParentMessageConsumer>(
                    c => c.OnDomainEventAsync));
                
            OnDomainEvent<MockDomainEventTwo>(
                route => route.ToConsumer<MockErrorChildMessageConsumer>(
                    c => c.OnDomainEventTwoAsync));
        }
    }
}