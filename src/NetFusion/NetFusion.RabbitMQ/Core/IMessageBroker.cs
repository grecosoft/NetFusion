using NetFusion.Messaging;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Interface exposed by the message broker providing
    /// only those methods that external components can use.
    /// </summary>
    public interface IMessageBroker
    {
        /// <summary>
        /// Determines if a message has an associated exchange.
        /// </summary>
        /// <param name="message">The type of the message.</param>
        /// <returns>True if there is an exchange defined for the 
        /// message.  Otherwise, False.</returns>
        bool IsExchangeMessage(IMessage message);

        /// <summary>
        /// Publishes a message to an exchange.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        void PublishToExchange(IMessage message);
    }
}
