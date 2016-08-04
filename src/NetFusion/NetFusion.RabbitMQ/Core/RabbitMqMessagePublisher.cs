using NetFusion.Messaging;
using NetFusion.Messaging.Core;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Extends to messaging pipeline by publishing methods associated with
    /// an exchange to the message bus.
    /// </summary>
    public class RabbitMqMessagePublisher : MessagePublisher
    {
        private readonly IMessageBroker _messageBroker;

        public RabbitMqMessagePublisher(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public override void PublishMessage(IMessage message)
        {
            if (_messageBroker.IsExchangeMessage(message))
            {
                _messageBroker.PublishToExchange(message);
            }
        }
    }
}
