using System;
using MessagePack.Resolvers;
using NetFusion.Base;
using NetFusion.Base.Serialization;

namespace NetFusion.Serialization
{
    /// <summary>
    /// Serializer for message pack serialization compact binary format.
    /// 
    /// http://msgpack.org/index.html
    /// </summary>
    public class MessagePackSerializer : IMessageSerializer
    {        
        public string ContentType => ContentTypes.MessagePack;

        public MessagePackSerializer()
        {
            MessagePack.MessagePackSerializer.SetDefaultResolver(ContractlessStandardResolver.Instance);
        }

        public object Deserialize(byte[] value, Type valueType)
        {
            return Deserialize<object>(value, valueType);
        }

        public byte[] Serialize(object value)
        {
            return MessagePack.MessagePackSerializer.Serialize(value);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            return (T)MessagePack.MessagePackSerializer.NonGeneric.Deserialize(valueType, value);
        }
    }
}

