using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NetFusion.Web.Common;

namespace NetFusion.Web.Rest.Client.Core;

/// <summary>
/// Serializes and Deserializes to and from JSON by delegating
/// to Microsoft's JsonSerializer.
/// </summary>
public class JsonMediaTypeSerializer : IMediaTypeSerializer
{
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonMediaTypeSerializer(JsonSerializerOptions options) 
        : this(InternetMediaTypes.Json, options)
    {

    }
        
    public JsonMediaTypeSerializer(string mediaType, JsonSerializerOptions options)
    {
        _serializerOptions = options ?? throw new ArgumentNullException(nameof(options));
            
        if (string.IsNullOrWhiteSpace(mediaType))
            throw new ArgumentException("Media-Type must be specified.", nameof(mediaType));
            
        MediaType = mediaType;
    
    }

    public string MediaType { get; }
        
        
    public byte[] Serialize(object value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
            
        return JsonSerializer.SerializeToUtf8Bytes(value, _serializerOptions);
    }

    public Task<T> Deserialize<T>(Stream responseStream)
    {
        if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
            
        return JsonSerializer.DeserializeAsync<T>(responseStream, _serializerOptions).AsTask();
    }

    public Task<object> Deserialize(Stream responseStream, Type type)
    {
        if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
        if (type == null) throw new ArgumentNullException(nameof(type));

        return JsonSerializer.DeserializeAsync(responseStream, type, _serializerOptions).AsTask();
    }
}