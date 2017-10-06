using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace NetFusion.RabbitMQ.Exchanges
{
    /// <summary>
    /// Defines the common default settings for a fanout exchange.  For this type
    /// of exchange, a message will be delivered to all queues bound to the exchange.
    /// The route-key is not used to determine message routing.  Usually, the consumers
    /// define temporary queues and only care about current messages.
    /// <typeparam name="TMessage">The message associated with the exchange.</typeparam>
    /// </summary>  
    public abstract class FanoutExchange<TMessage> : MessageExchange<TMessage>
        where TMessage : IMessage
    {
        public FanoutExchange()
        {
            Settings.ExchangeType = ExchangeType.Fanout;
            Settings.IsDurable = true;
            Settings.IsAutoDelete = false;

            // Default queue settings.  However, queues are not usually created at the time of exchange
            // creation.  Fanout queues are normally created by connected clients that are interested 
            // in notifications.
            QueueSettings.IsDurable = false;    
            QueueSettings.IsNoAck = true;       // Consumer not required to acknowledge message.
            QueueSettings.IsExclusive = true;   // A queue is created per client and can only be monitored by that client.
            QueueSettings.IsAutoDelete = true;  
        }
    }
}