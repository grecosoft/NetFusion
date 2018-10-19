using Amqp;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Publisher.Internal
{
    /// <summary>
    /// Represents an object defined on an Azure namespace with the
    /// associated AMQP links used the send and receive messages.
    /// </summary>
    public interface ILinkedItem
    {
        /// <summary>
        /// The name of the object defined on the namespace.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The AMQP link used to send messages to the namespace object.
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