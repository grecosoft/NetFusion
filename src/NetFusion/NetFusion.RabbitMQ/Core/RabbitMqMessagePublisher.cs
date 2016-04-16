using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.RabbitMQ.Modules;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Extends to messaging pipeline by publishing methods associated with
    /// an exchange to the message bus.
    /// </summary>
    public class RabbitMqMessagePublisher : MessagePublisher
    {
        private readonly IMessageBrokerModule _eventBrokerModule;

        public RabbitMqMessagePublisher(IMessageBrokerModule messageBrokerModule)
        {
            _eventBrokerModule = messageBrokerModule;
        }

        public override void PublishMessage(IMessage message)
        {
            if (_eventBrokerModule.MessageBroker.IsExchangeMessage(message))
            {
                _eventBrokerModule.MessageBroker.PublishToExchange(message);
            }
        }
    }
}
