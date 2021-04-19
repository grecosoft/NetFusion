using System;
using System.Threading.Tasks;
using CoreTests.Messaging.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Provides a basic set up for testing publishing of domain-events.
    /// </summary>
    public static class TestSetupExtensions
    {
        // Adds a host plugin configured with the core messaging plugin.
        public static CompositeContainer AddHost(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
        }
        
        // ----------------------- [Exception Test Scenarios] --------------------------
        
        public static CompositeContainer WithMessageHandlerException(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockErrorMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        public static CompositeContainer WithChildMessageHandlerException(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockErrorParentMessageConsumer>();
            appPlugin.AddPluginType<MockErrorChildMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }
        
        
        
        
        
        
        
        
        
        
        public static CompositeContainer WithHostConsumer(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<MockDomainEventConsumer>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();

            return container;
        }

        

        public static CompositeContainer AddDerivedEventAndConsumer(this CompositeContainer container)
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<MockBaseMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }

        

        
    }

    //-------------------------- MOCKED TYPED --------------------------------------

    /// <summary>
    /// Basic message handler used to test common consuming of messages.
    /// </summary>
    public class MockDomainEventConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void OnEventHandlerOne(MockDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            AddCalledHandler("OnEventHandlerOne");
        }
    }

    /// <summary>
    /// Message handler that can be used to test exceptions that are
    /// raised when consuming a message.
    /// </summary>
    public class MockErrorMessageConsumer : IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEventAsync(MockDomainEvent evt)
        {
            return Task.Run(() => throw new InvalidOperationException(nameof(OnEventAsync)));
        }
    }
    
    public class MockErrorParentMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        private readonly IMessagingService _messaging;
        
        public MockErrorParentMessageConsumer(IMessagingService messaging)
        {
            _messaging = messaging;
        }
        
        [InProcessHandler]
        public async Task OnDomainEventAsync(MockDomainEvent evt)
        {
            await _messaging.PublishAsync(new MockDomainEventTwo());
        }
    }

    public class MockErrorChildMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public Task OnDomainEventTwoAsync(MockDomainEventTwo evt)
        {
            return Task.Run(() => throw new InvalidOperationException(nameof(OnDomainEventTwoAsync)));
        }
    }
    
    
    
    


    public class MockBaseDomainEvent : DomainEvent
    {

    }

    public class MockDerivedDomainEvent : MockBaseDomainEvent
    {
    }

    public class MockBaseMessageConsumer : MockConsumer,
        IMessageConsumer
    {

        [InProcessHandler]
        public void OnBaseEventHandler(MockBaseDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            AddCalledHandler("OnBaseEventHandler");
        }

        [InProcessHandler]
        public void OnIncludeBaseEventHandler([IncludeDerivedMessages]MockBaseDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            AddCalledHandler("OnIncludeBaseEventHandler");
        }

        
    }
}
