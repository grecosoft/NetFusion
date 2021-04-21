using System.Collections.Generic;
using NetFusion.Messaging.Types.Contracts;

namespace CoreTests.Messaging.Mocks
{
    /// <summary>
    /// Message consumer that records the called message
    /// handler methods.
    /// </summary>
    public abstract class MockConsumer
    {
        private readonly List<string> _executedHandlers = new ();
        private readonly List<IMessage> _receivedMessages = new();

        protected MockConsumer()
        {
            ExecutedHandlers = _executedHandlers;
            ReceivedMessages = _receivedMessages;
        }

        public IReadOnlyCollection<string> ExecutedHandlers { get; }
        public IReadOnlyCollection<IMessage> ReceivedMessages { get; }

        protected void AddCalledHandler(string handlerName)
        {
            _executedHandlers.Add(handlerName);
        }

        protected void RecordReceivedMessage(IMessage message) => _receivedMessages.Add(message);
    }
}
