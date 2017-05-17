using NetFusion.Base.Plugins;
using System;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Implementations determine how a value is serialized when
    /// published to an exchange and deserialized when received on a
    /// queue bound to the exchange.
    /// </summary>
    public interface IBrokerSerializer : IKnownPluginType
    {
        /// <summary>
        /// The content type (example: application/json).
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Returns a serialized value that can be published to an exchange.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>Serialized value.</returns>
        byte[] Serialize(object value);

        /// <summary>
        /// Returns a value that has been deserialized.  Used to 
        /// deserialize a message received on a queue,
        /// </summary>
        /// <param name="value">Byte array containing serialized value.</param>
        /// <param name="valueType">The type of the value contained within
        /// the serialized byte array.</param>
        /// <returns>Instance of the value.</returns>
        object Deserialize(byte[] value, Type valueType);

        /// <summary>
        /// Returns a value that has been deserialized.  Used to 
        /// deserialize a message received on a queue,</summary>
        /// <typeparam name="T">The type to case the deserialized result.</typeparam>
        /// <param name="value">Byte array containing serialized value.</param>
        /// <param name="valueType">The type of the value contained within
        /// the serialized byte array.</param>
        /// <returns>Instance of the value.</returns>
        T Deserialize<T>(byte[] value, Type valueType);
    }
}
