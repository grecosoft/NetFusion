using NetFusion.Messaging;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Defines the common default settings for a direct exchange.  For this type of
    /// exchange, a message will be delivered to a bound queue if its route-key matches
    /// the route-key value of the published message.
    /// <typeparam name="TMessage">The message associated with the exchange.</typeparam>
    /// </summary>
    public abstract class DirectExchange<TMessage> : MessageExchange<TMessage>
        where TMessage : IMessage
    {
        public DirectExchange()
        {
            Settings.ExchangeType = ExchangeType.Direct;
            Settings.IsDurable = true;
            Settings.IsAutoDelete = false;

            QueueSettings.IsDurable = true;
            QueueSettings.IsNoAck = false;      // Require consumer to acknowledge message.
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
