using System;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Implementations determine how a message is serialized when
    /// published to an exchange and deserialized when received on a
    /// queue bound to the exchange.
    /// </summary>
    public interface IMessageSerializer : IKnownPluginType
    {
        /// <summary>
        /// Description of the content type (example: application/json).
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Returns the message serialized and encoded to a byte array.
        /// </summary>
        /// <param name="domainEntity">The message to serialize.</param>
        /// <returns>Serialized message that will become the message's
        /// body when published to the exchange.</returns>
        byte[] Serialize(IMessage domainEntity);

        /// <summary>
        /// Returns a message recreated from its corresponding serialized 
        /// and encoded byte array.
        /// </summary>
        /// <param name="message">Byte array containing serialized message.</param>
        /// <param name="messageType">The type of the message contained within
        /// the serialized byte array.</param>
        /// <returns>Instance of the re-created message.</returns>
        IMessage Deserialize(byte[] message, Type messageType);
    }
}
