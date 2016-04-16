using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockPrefixedMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        public void OnEventHandlerOne(MockDomainEvent domainEvent)
        {
            AddCalledHandler("OnEventHandlerOne");
        }

        public void WhenEventHandlerTwo(MockDomainEvent domainEvent)
        {
            AddCalledHandler("WhenEventHandlerTwo");
        }
    }
}
