using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockDomainEventConsumer : MockConsumer,
        IMessageConsumer
    {
        public void OnEventHandlerOne(MockDomainEvent domainEvent)
        {
            AddCalledHandler("OnEventHandlerOne");
        }

    }
}
