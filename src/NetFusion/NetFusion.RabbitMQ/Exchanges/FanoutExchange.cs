using NetFusion.Messaging;
using RabbitMQ.Client;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Defines the common default settings for a fanout exchange.  For this type
    /// of exchange, a message will be delivered to all queues bound to the exchange.
    /// The route-key is not used to determine message routing.  Usually, the consumers
    /// define temporary queues and only care about current messages.
    /// <typeparam name="TMessage">The message associated with the exchange.</typeparam>
    public abstract class FanoutExchange<TMessage> : MessageExchange<TMessage>
        where TMessage : IMessage
    {
        public FanoutExchange()
        {
            Settings.ExchangeType = ExchangeType.Fanout;
            Settings.IsDurable = true;
            QueueSettings.IsNoAck = true;   // Consumer don't have to acknowledge message.
        }
    }
}