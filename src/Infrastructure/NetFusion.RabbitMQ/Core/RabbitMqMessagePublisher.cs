using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using System.Threading;
using System.Threading.Tasks;

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

        public override IntegrationTypes IntegrationType => IntegrationTypes.External;

        // Determine if the message being published should be submitted to an 
        // exchange/queue for processing by a consumer.  If the message is a
        // RPC style message, the request is made to the consumer's queue and
        // the response is awaited by the client.
        public override Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            if (_messageBroker.IsExchangeMessage(message))
            {
                return _messageBroker.PublishToExchangeAsync(message);
            } 
            else if(_messageBroker.IsRpcCommand(message))
            {
                return _messageBroker.PublishToRpcConsumerAsync(message, cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
