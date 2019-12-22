using System;
using NetFusion.Rest.Client.Settings;
using NetFusion.Rest.Common;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client.Core
{
    /// <summary>
    /// Serializes and Deserializes to and from JSON.
    /// </summary>
    public class JsonMediaTypeSerializer : IMediaTypeSerializer
    {
        private JsonSerializerOptions _serializerOptions;
        
        public void Initialize(ClientSettings settings)
        {
            _serializerOptions = settings.SerializerOptions;
        }

        public string MediaType => InternetMediaTypes.Json;
        
        
        public byte[] Serialize(object value)
        {
            string json = JsonSerializer.Serialize(value);
            return Encoding.UTF8.GetBytes(json);

        }

        public Task<T> Deserialize<T>(Stream responseStream)
        {
            return JsonSerializer.DeserializeAsync<T>(responseStream, _serializerOptions).AsTask();
        }
        
        public Task<object> Deserialize(Stream responseStream, Type type)
        {
            return JsonSerializer.DeserializeAsync(responseStream, type, _serializerOptions).AsTask();
        }
    }
}