using CoreTests.Messaging.Mocks;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using NetFusion.Test.Plugins;
using System;
using System.Threading.Tasks;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Provides a basic set up for testing publishing of domain-events.
    /// </summary>
    public static class TestSetupExtensions
    {
        public static TestTypeResolver WithHostConsumer(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType<MockDomainEvent>()
                .AddPluginType<MockDomainEventConsumer>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseMessagingPlugin();

            return resolver;
        }

        public static TestTypeResolver WithHost(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseMessagingPlugin();

            return resolver;
        }

        public static TestTypeResolver AddDerivedEventAndConsumer(this TestTypeResolver resolver)
        {
            resolver.GetHostPlugin()
                .AddPluginType<MockDerivedDomainEvent>()
                .AddPluginType<MockBaseMessageConsumer>();

            return resolver;
        }

        public static TestTypeResolver AddEventAndExceptionConsumer(this TestTypeResolver resolver)
        {
            resolver.GetHostPlugin()
              .AddPluginType<MockDomainEvent>()
              .AddPluginType<MockErrorMessageConsumer>();

            return resolver;
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
