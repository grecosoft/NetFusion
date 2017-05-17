#if NET461

using System;
using NetFusion.Base;
using NetFusion.Messaging;
using NetFusion.Common.Serialization;
using NetFusion.Common;
using NetFusion.Domain.Messaging;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializes a value to Binary representation.  Called by the
    /// MessageBroker when serializing and deserializing messages
    /// to queues.
    /// </summary>
    public class BinaryBrokerSerializer : IBrokerSerializer
    {
        public string ContentType => SerializerTypes.Binary;

        public byte[] Serialize(object value)
        {
            Check.NotNull(value, nameof(value));
            return BinaryFormatterUtility.Serialize(value);
        }

        public object Deserialize(byte[] value, Type valueType)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(valueType, nameof(valueType));

            return BinaryFormatterUtility.Deserialize<IMessage>(value);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            return (T)Deserialize(value, valueType);
        }
    }
}

#endif