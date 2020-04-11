using System;
using System.IO;
using MsgPack.Serialization;
using NetFusion.Base;
using NetFusion.Base.Serialization;
using MsgPackCliSerializer = MsgPack.Serialization.MessagePackSerializer;

namespace NetFusion.Serialization
{
    /// <summary>
    /// Serializer for message pack serialization compact binary format.
    /// </summary>
    public class MessagePackSerializer : IMessageSerializer
    {        
        public string ContentType => ContentTypes.MessagePack;
        public string EncodingType => null;
        
        public object Deserialize(byte[] value, Type valueType)
        {
            return Deserialize<object>(value, valueType);
        }

        public byte[] Serialize(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var serializer = MsgPackCliSerializer.Get(value.GetType());
            var memoryStream = new MemoryStream();
            
            serializer.Pack(memoryStream, value);
            return memoryStream.ToArray();
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            
            var serializer = MsgPackCliSerializer.Get(valueType);
            var memoryStream = new MemoryStream(value);

            return (T) serializer.Unpack(memoryStream);
        }
    }
}

