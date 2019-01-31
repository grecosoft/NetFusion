using Amqp;
using NetFusion.Messaging.Types;

namespace NetFusion.AMQP.Publisher.Internal
{
    /// <summary>
    /// Represents an item defined on the host that can receive
    /// sent messages.
    /// </summary>
    public interface ISenderHostItem
    {
        /// <summary>
        /// The name of the item defined on the host that can receive messages.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The AMQP link used to send messages to the host item.
        /// </summary>
        ISenderLink SenderLink { get; set; }

        /// <summary>
        /// Called before a message is published to determine if it matches
        /// the criteria of the defined item.  
        /// </summary>
        /// <param name="message">The message to test.</param>
        /// <returns>True if the message applies.  Otherwise, false.</returns>
        bool MessageApplies(IMessage message);

        /// <summary>
        /// Creates a AMQP message from the provided body and sets any related
        /// message properties.
        /// </summary>
        /// <param name="message">The message contained in the serialized body.</param>
        /// <param name="body">The serialized body of the message.</param>
        /// <returns>The created message with populated message properties.</returns>
        Message CreateMessage(IMessage message, object body);
    }
}