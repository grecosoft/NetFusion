using System;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Attribute used to specify that a consumer's method should be invoked when 
    /// a message is published to a fan-out exchange.  This is for messages that
    /// represent a type of notification where only those received after the client
    /// starts is necessary.  Any messages sent while the client is off line, will
    /// not be received.
    /// </summary>
    public class AddFanoutQueueAttribute : QueueConsumerAttribute
    {
        /// <summary>
        /// Indicates a method should be invoked when a message
        /// is published to a fan-out exchange.
        /// </summary>
        /// <param name="exchangeName">The name of the exchange.</param>
        public AddFanoutQueueAttribute(string exchangeName) :
            base(null, exchangeName, QueueBindingTypes.Create)
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name must be specified.", nameof(exchangeName));

            QueueSettings.IsAutoDelete = true;
            QueueSettings.IsDurable = false;
            QueueSettings.IsNoAck = true;
            QueueSettings.IsExclusive = true;
        }
    }
}
