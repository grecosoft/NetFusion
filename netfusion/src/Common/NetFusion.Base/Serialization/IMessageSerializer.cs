using System;
using NetFusion.Base.Plugins;

namespace NetFusion.Base.Serialization
{
    /// <summary>
    /// Implementations determine how an object is serialized and deserialized.
    /// </summary>
    public interface IMessageSerializer : IKnownPluginType
    {
        /// <summary>
        /// The content type (example: application/json).
        /// </summary>
        string ContentType { get; }
        
        /// <summary>
        /// The optional encoding of the serialized data.
        /// </summary>
        string EncodingType { get; }

        /// <summary>
        /// Returns serialized representation of the object.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <returns>Serialized value.</returns>
        byte[] Serialize(object value);

        /// <summary>
        /// Returns a value that has been deserialized from its serialized representation.
        /// </summary>
        /// <param name="value">Byte array containing serialized value.</param>
        /// <param name="valueType">The type of the value contained within
        /// the serialized byte array.</param>
        /// <returns>Instance of the value.</returns>
        object Deserialize(byte[] value, Type valueType);

        /// <summary>
        /// Returns a value that has been deserialized from its serialized representation.</summary>
        /// <typeparam name="T">The type to case the deserialized result.</typeparam>
        /// <param name="value">Byte array containing serialized value.</param>
        /// <param name="valueType">The type of the value contained within
        /// the serialized byte array.</param>
        /// <returns>Instance of the value.</returns>
        T Deserialize<T>(byte[] value, Type valueType);
    }
}