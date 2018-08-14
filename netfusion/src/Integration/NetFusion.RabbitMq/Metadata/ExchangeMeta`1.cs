using System;
using NetFusion.Messaging.Types;

namespace NetFusion.RabbitMQ.Metadata
{
    /// <summary>
    /// ExchangeMeta derived class for a specified type of message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class ExchangeMeta<TMessage> : ExchangeMeta
        where TMessage : IMessage
    {
        public ExchangeMeta()
        {
            MessageType = typeof(TMessage);
        }

        /// <summary>
        /// Overrides the base implementation that invokes a message
        /// type specific delegate determining if the message applies.
        /// </summary>
        /// <param name="message">The message being published to exchange.</param>
        /// <returns>True if message applies.  Otherwise, false.</returns>
        internal override bool Applies(IMessage message)
        {
            return AppliesIf((TMessage)message);
        }

        /// <summary>
        /// Delegate that can be specified within code to determine if the 
        /// state of the message applied to the exchange and should be published.
        /// </summary>
        /// <returns>True if message applies.  Otherwise, false.</returns>
        public Func<TMessage, bool> AppliesIf = m => true;
    }
}