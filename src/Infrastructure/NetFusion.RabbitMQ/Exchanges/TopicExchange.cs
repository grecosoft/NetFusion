using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Defines the common default settings for a topic exchange.  For this type of
    /// exchange, a message will be delivered to a queue if the route-key for the
    /// published message matches the filter specified by the queue's exchange
    /// binding.
    /// </summary>
    /// <typeparam name="TMessage">The message associated with the exchange.</typeparam>
    public abstract class TopicExchange<TMessage> : MessageExchange<TMessage>
        where TMessage : IMessage
    {
        public TopicExchange()
        {
            Settings.ExchangeType = ExchangeType.Topic;
            Settings.IsDurable = true;
            Settings.IsAutoDelete = false;

            QueueSettings.IsDurable = true;
            QueueSettings.IsNoAck = false;   // Require consumer to acknowledge message.
            QueueSettings.IsExclusive = false;  // Multiple clients can monitor queue.
            QueueSettings.IsAutoDelete = false;
        }

        internal override void ValidateConfiguration()
        {
            base.ValidateConfiguration();
            ValidateRequiredRouteKey();
        }

        protected override void OnSetPublisherBasicProperties(IModel channel, IMessage message, IBasicProperties properties)
        {
            // Make the message persistent.
            properties.Persistent = true;
        }
    }
}