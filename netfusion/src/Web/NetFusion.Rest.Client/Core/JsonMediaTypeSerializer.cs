using System;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Serializes and Deserializes to and from JSON.
    /// </summary>
    public class JsonMediaTypeSerializer : IMediaTypeSerializer
    {
        public string MediaType => InternetMediaTypes.Json;

        public byte[] Serialize(object value)
        {
            string json = JsonConvert.SerializeObject(value);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(Stream responseStream)
        {
            var serializer = new JsonSerializer();
            using (var reader = new StreamReader(responseStream))
            {
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }
        
        public object Deserialize(Stream responseStream, Type type)
        {
            var serializer = new JsonSerializer();
            using (var reader = new StreamReader(responseStream))
            {
                return serializer.Deserialize(reader, type);
            }
        }
    }
}
