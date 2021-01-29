using System;
using System.Text;
using System.Text.Json;
using NetFusion.Base;
using NetFusion.Base.Serialization;

namespace NetFusion.Serialization
{
    /// <summary>
    /// Serializes a value to JSON representation. 
    /// </summary>
    public class JsonMessageSerializer : IMessageSerializer
    {
        public string ContentType => ContentTypes.Json;
        public string EncodingType => Encoding.UTF8.WebName;

        public byte[] Serialize(object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            
            return JsonSerializer.SerializeToUtf8Bytes(value);
        }

        public object Deserialize(byte[] value, Type valueType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));

            return JsonSerializer.Deserialize(value, valueType);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            return (T)Deserialize(value, valueType);
        }
    }
}
