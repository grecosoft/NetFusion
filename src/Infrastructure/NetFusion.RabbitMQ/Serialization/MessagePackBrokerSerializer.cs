using MsgPack.Serialization;
using NetFusion.Base;
using System;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializer for message pack serialization compact binary format.
    /// http://msgpack.org/index.html
    /// </summary>
    public class MessagePackBrokerSerializer : IBrokerSerializer
    {
        public string ContentType => SerializerTypes.MessagePack;

        public object Deserialize(byte[] value, Type valueType)
        {
            return Deserialize<object>(value, valueType);
        }

        public byte[] Serialize(object value)
        {
            var serializer = SerializationContext.Default.GetSerializer(value.GetType());
            return serializer.PackSingleObject(value);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            var serializer = SerializationContext.Default.GetSerializer(valueType);
            return (T)serializer.UnpackSingleObject(value);
        }
    }
}

