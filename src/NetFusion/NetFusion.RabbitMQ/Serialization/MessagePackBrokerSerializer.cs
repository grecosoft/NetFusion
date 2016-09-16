using MsgPack.Serialization;
using System;

namespace NetFusion.RabbitMQ.Serialization
{
    public class MessagePackBrokerSerializer : IBrokerSerializer
    {
        public string ContentType => SerializerTypes.MessagePack;

        public object Deserialize(byte[] value, Type valueType)
        {
            throw new NotImplementedException();
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
