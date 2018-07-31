using MessagePack;
using NetFusion.Base;
using System;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializer for message pack serialization compact binary format.
    /// http://msgpack.org/index.html
    /// </summary>
    public class MessagePackBrokerSerializer : IMessageSerializer
    {        
        public string ContentType => SerializerTypes.MessagePack;

        public object Deserialize(byte[] value, Type valueType)
        {
            return Deserialize<object>(value, valueType);
        }

        public byte[] Serialize(object value)
        {
            return MessagePackSerializer.Serialize(value);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            return (T)MessagePackSerializer.NonGeneric.Deserialize(valueType, value);
        }
    }
}

