using NetFusion.Common;
using NetFusion.Messaging;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Message exchange for which specific message types
    /// can be published.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageExchange<TMessage> : BrokerExchange
        where TMessage : IMessage
    {

        public MessageExchange()
        {
            this.MessageType = typeof(TMessage);
        }

        public override bool Matches(IMessage message)
        {
            Check.NotNull(message, nameof(message));
            return Matches((TMessage)message);
        }

        /// <summary>
        /// Can be specified by a derived exchange to determine if the message
        /// passes the criteria required to be published to the exchange.
        /// </summary>
        /// <param name="message">The message about to be published to the
        /// exchange.</param>
        /// <returns>True if the message should be published to the exchange.
        /// Otherwise, false.</returns>
        protected virtual bool Matches(TMessage message)
        {
            return true;
        }
    }
}
