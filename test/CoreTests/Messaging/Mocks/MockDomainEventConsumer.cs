using NetFusion.Common;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;

namespace CoreTests.Messaging.Mocks
{
    /// <summary>
    /// Basic message handler used to test common consuming of messages.
    /// </summary>
    public class MockDomainEventConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void OnEventHandlerOne(MockDomainEvent domainEvent)
        {
            Check.NotNull(domainEvent, nameof(domainEvent));
            AddCalledHandler("OnEventHandlerOne");
        }
    }
}
