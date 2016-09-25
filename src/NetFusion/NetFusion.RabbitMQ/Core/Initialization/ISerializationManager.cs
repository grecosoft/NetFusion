using NetFusion.Messaging;
using NetFusion.RabbitMQ.Serialization;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Implements serialization/deserialization based on a set
    /// of IBrokerSerializer instances that are each associated
    /// with a specific content-type.
    /// </summary>
    public interface ISerializationManager
    {
        /// <summary>
        /// The list of configured serializers.
        /// </summary>
        IEnumerable<IBrokerSerializer> Serializers { get; }

        /// <summary>
        /// Serializes an object based on the specified content-type.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="contentType">The content-type used to lookup serializer.</param>
        /// <returns>Serialized byte array.</returns>
        byte[] Serialize(object value, string contentType);

        /// <summary>
        /// Serializes a message based on the specified ordered list of content types.
        /// </summary>
        /// <param name="message">The message to serialize.</param>
        /// <param name="contentTypes">List of content-types in priority order.  
        /// The first non-null content-type is used.</param>
        /// <returns>Serialized byte array.</returns>
        byte[] Serialize(IMessage message, params string[] contentTypes);

        /// <summary>
        /// Deserializes the message body associated with a delivery event into
        /// a message object.
        /// </summary>
        /// <param name="messageType">The type of the message contained within the
        /// serialized event body.</param>
        /// <param name="deliveryEvent">The event contained the delivered serialized
        /// contents.</param>
        /// <returns>The corresponding message object.</returns>
        IMessage Deserialize(Type messageType, BasicDeliverEventArgs deliveryEvent);

        /// <summary>
        /// Deserializes a byte array into the specified value type using the
        /// specified content-type.
        /// </summary>
        /// <param name="contentType">The content-type of the serialized data.</param>
        /// <param name="valueType">The type the data should be deserialized into.</param>
        /// <param name="value">The data for the value.</param>
        /// <returns>Deserialized object of the specified type.</returns>
        object Deserialize(string contentType, Type valueType, byte[] value);
    }
}
