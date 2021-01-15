using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher.Internal
{
    /// <summary>
    /// Interface implemented by namespace entities to determine
    /// a published message should be deleted to the entity.
    /// </summary>
    public interface IMessageFilter
    {
        /// <summary>
        /// Invoked when message is published to determine if it should
        /// be delivered to the entity.
        /// </summary>
        /// <param name="message">The published entity.</param>
        /// <returns>Return true if the message should be delivered.
        /// Otherwise, false if message does not apply.</returns>
        bool Applies(IMessage message);
    }
}