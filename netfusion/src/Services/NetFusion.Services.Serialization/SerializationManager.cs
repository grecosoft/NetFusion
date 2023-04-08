using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Common.Base.Serialization;

namespace NetFusion.Services.Serialization;

/// <summary>
/// Encapsulates the logic for serializing objects using a set of serializers registered
/// by content-type.  By default, JSON and MessagePack based serializers are  registered.
/// 
/// A consuming application can override these default serializers by instantiating this 
/// class and calling the AddSerializer method to override the serializer to use for a
/// specific content-type.  Also, the consuming application can call the ClearSerializers
/// method to clear all default registrations before adding custom serializers.  
/// 
/// The instance of this class should be registered as a singleton service type of 
/// ISerializationManager.
/// </summary>
public class SerializationManager : ISerializationManager
{
    // Serializers searched by content-type and encoding-type:
    private readonly List<ISerializer> _serializers;

    public SerializationManager()
    {
        _serializers = new List<ISerializer>();

        AddSerializer(new JsonMessageSerializer());
        AddSerializer(new MessagePackSerializer());
    }

    public IEnumerable<ISerializer> Serializers => _serializers;

    public void ClearSerializers() => _serializers.Clear();

    public void AddSerializer(ISerializer serializer)
    {
        if (serializer == null) throw new ArgumentNullException(nameof(serializer));
        
        var existing = _serializers.FirstOrDefault(s =>
            s.ContentType == serializer.ContentType && 
            s.EncodingType == serializer.EncodingType);

        if (existing != null)
        {
            _serializers.Remove(existing);
        }
            
        _serializers.Add(serializer);
    }

    public byte[] Serialize(object value, string contentType, string? encodingType = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type name must be specified.", nameof(contentType));
            
        ISerializer serializer = GetSerializer(contentType, encodingType);
        return serializer.Serialize(value);
    }

    public object? Deserialize(string contentType, Type valueType, byte[] value, string? encodingType = null)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content-Type must be specified.", nameof(contentType));

        if (valueType == null) throw new ArgumentNullException(nameof(valueType));
        if (value == null) throw new ArgumentNullException(nameof(value));

        ISerializer serializer = GetSerializer(contentType, encodingType);
        return serializer.Deserialize(value, valueType);
    }

    public T? Deserialize<T>(string contentType, byte[] value, string? encodingType = null)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content-Type must be specified.", nameof(contentType));

        if (value == null) throw new ArgumentNullException(nameof(value));

        ISerializer serializer = GetSerializer(contentType, encodingType);
        return (T?)serializer.Deserialize(value, typeof(T));
    }

    public ISerializer GetSerializer(string contentType, string? encodingType = null)
    {
        var typeProps = GetContentTypeAndEncoding(contentType, encodingType);
            
        var matchingSerializers = _serializers.Where(s => s.ContentType == typeProps.contentType).ToArray();
        if (!matchingSerializers.Any())
        {
            throw new SerializationException(
                $"No serializers found for Content-Type: {typeProps.contentType}.",
                "CONTENT_TYPE_NOT_FOUND");
        }

        // Find an exact match:
        if (typeProps.encodingType != null)
        {
            var serializer = matchingSerializers.FirstOrDefault(s => s.EncodingType == typeProps.encodingType);
            if (serializer == null)
            {
                throw new SerializationException(
                    $"Serializer for Content-Type: {contentType} Encoding-Type: {typeProps.encodingType} not registered.",
                    "ENCODING_NOT_FOUND");
            }
            
            return serializer;
        }
        
        if (matchingSerializers.Length > 1)
        {
            throw new SerializationException(
                $"Multiple serializers found for Content-Type: {typeProps.contentType}. Encoding type must be specified.",
                "MULTIPLE_SERIALIZERS_FOUND");
        }
        
        return matchingSerializers.First();
    }

    // If the encoding-type is not explicitly specified, determines if the content-type
    // values specifies the encoding.  Example: application/json; charset=utf-8
    private static (string contentType, string? encodingType) GetContentTypeAndEncoding(
        string contentType, string? encodingType)
    {
        encodingType = string.IsNullOrEmpty(encodingType) ? null : encodingType;            
        if (encodingType != null)
        {
            return (contentType, encodingType);
        }

        // Parse the encoding type if present in the content-type:
        var value = contentType.Replace(" ", "");
        var parts = value.Split(';');

        if (parts.Length != 2)
        {
            return (contentType, null);
        }

        return parts[1].Contains("charset") ? (parts[0], parts[1].Replace("charset=", "")) 
            : (contentType, null);
    }
}