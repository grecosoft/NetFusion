using NetFusion.Common;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Attribute used to specify that a consumer's method 
    /// should be invoked when a message is published to a
    /// fan-out exchange.
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
            Check.NotNullOrWhiteSpace(exchangeName, nameof(exchangeName));

            this.QueueSettings.IsAutoDelete = true;
            this.QueueSettings.IsDurable = false;
            this.QueueSettings.IsNoAck = true;
            this.QueueSettings.IsExclusive = true;
        }
    }
}
