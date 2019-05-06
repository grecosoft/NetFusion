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
        public static CompositeContainer WithHostConsumer(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            hostPlugin.AddPluginType<MockDomainEventConsumer>();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();

            return container;
        }

        public static CompositeContainer WithHost(this CompositeContainer container)
        {
            var hostPlugin = new MockHostPlugin();
            
            container.RegisterPlugins(hostPlugin);
            container.RegisterPlugin<MessagingPlugin>();
            
            return container;
        }

        public static CompositeContainer AddDerivedEventAndConsumer(this CompositeContainer container)
        {
            var appPlugin = new MockApplicationPlugin();
            appPlugin.AddPluginType<MockBaseMessageConsumer>();
            
            container.RegisterPlugins(appPlugin);
            return container;
        }

        public static CompositeContainer AddEventAndExceptionConsumer(this CompositeContainer container)
        {
            var appPlugin = new MockApplicationPlugin();
            appPlugin.AddPluginType<MockErrorMessageConsumer>();
            
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
        public Task OnEvent1Async(MockDomainEvent evt)
        {
            return Task.Run(() => throw new InvalidOperationException(nameof(OnEvent1Async)));
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
