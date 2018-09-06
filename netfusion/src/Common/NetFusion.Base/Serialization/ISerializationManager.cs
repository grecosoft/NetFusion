using System;
using System.Collections.Generic;
using NetFusion.Base.Plugins;

namespace NetFusion.Base.Serialization
{
    /// <summary>
    /// Implements serialization/deserialization based on a set
    /// of IMessageSerializer instances that are each associated
    /// with a specific content-type.
    /// </summary>
    public interface ISerializationManager : IKnownPluginType
    {
        /// <summary>
        /// The list of configured serializers.
        /// </summary>
        IEnumerable<IMessageSerializer> Serializers { get; }

        /// <summary>
        /// Serializes an object based on the specified content-type.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="contentType">The content-type used to lookup serializer.</param>
        /// <returns>Serialized byte array.</returns>
        byte[] Serialize(object value, string contentType);

        /// <summary>
        /// Deserializes a byte array into the specified value type using the
        /// specified content-type.
        /// </summary>
        /// <param name="contentType">The content-type of the serialized data.</param>
        /// <param name="valueType">The type the data should be deserialized into.</param>
        /// <param name="value">The data for the value.</param>
        /// <returns>Deserialized object of the specified type.</returns>
        object Deserialize(string contentType, Type valueType, byte[] value);

        /// <summary>
        /// Deserializes a byte array into the specified value type using the
        /// specified content-type.
        /// </summary>
        /// <typeparam name="T">The type of the data should be deserialized into.</typeparam>
        /// <param name="contentType">The content-type of the serialized data.</param>
        /// <param name="value">The data for the value.</param>
        /// <returns>Deserialized object of the specified type.</returns>
        T Deserialize<T>(string contentType, byte[] value);
    }
}
