using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockPrefixedMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void OnEventHandlerOne(MockDomainEvent domainEvent)
        {
            AddCalledHandler("OnEventHandlerOne");
        }

        [InProcessHandler]
        public void WhenEventHandlerTwo(MockDomainEvent domainEvent)
        {
            AddCalledHandler("WhenEventHandlerTwo");
        }
    }
}
