#if NET461

using NetFusion.Base;
using NetFusion.Common;
using NetFusion.Common.Serialization;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;
using System;

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
            if (value == null) throw new ArgumentNullException(nameof(value));
            return BinaryFormatterUtility.Serialize(value);
        }

        public object Deserialize(byte[] value, Type valueType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return BinaryFormatterUtility.Deserialize<IMessage>(value);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            return (T)Deserialize(value, valueType);
        }
    }
}

#endif