using System.Threading.Tasks;
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

        public async override Task PublishMessageAsync(IMessage message)
        {
            if (_messageBroker.IsExchangeMessage(message))
            {
                await _messageBroker.PublishToExchange(message);
            } 
            else if(_messageBroker.IsRpcCommand(message))
            {
                await _messageBroker.PublishToRpcConsumer(message);
            }
        }
    }
}
