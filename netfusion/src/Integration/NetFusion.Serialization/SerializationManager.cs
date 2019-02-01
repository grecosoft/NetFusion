using System;
using System.Linq;
using System.Collections.Generic;
using NetFusion.Base.Serialization;

namespace NetFusion.Serialization
{
    /// <summary>
    /// Encapsulates the logic for serializing messages using a set of serializers registered
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
        // Serializer keyed by content-type:
        private readonly List<IMessageSerializer> _serializers;

        public SerializationManager()
        {
            _serializers = new List<IMessageSerializer>();

            AddSerializer(new JsonMessageSerializer());
            AddSerializer(new MessagePackSerializer());
        }

        public IEnumerable<IMessageSerializer> Serializers => _serializers;

        public void ClearSerializers() => _serializers.Clear();

        public void AddSerializer(IMessageSerializer serializer)
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

        public byte[] Serialize(object value, string contentType, string encodingType = null)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type name must be specified.", nameof(contentType));
            
            IMessageSerializer serializer = GetSerializer(contentType, encodingType);
            return serializer.Serialize(value);
        }

        public object Deserialize(string contentType, Type valueType, byte[] value, string encodingType = null)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content-Type must be specified.", nameof(contentType));

            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            if (value == null) throw new ArgumentNullException(nameof(value));

            IMessageSerializer serializer = GetSerializer(contentType, encodingType);
            return serializer.Deserialize(value, valueType);
        }

        public T Deserialize<T>(string contentType, byte[] value, string encodingType = null)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content-Type must be specified.", nameof(contentType));

            if (value == null) throw new ArgumentNullException(nameof(value));

            IMessageSerializer serializer = GetSerializer(contentType, encodingType);
            return serializer.Deserialize<T>(value, typeof(T));
        }

        public IMessageSerializer GetSerializer(string contentType, string encodingType = null)
        {
            var types = GetContentTypeAndEncoding(contentType, encodingType);
            
            var matchingSerializers = _serializers.Where(s => s.ContentType == types.contentType).ToArray();
            if (types.encodingType == null)
            {
                if (matchingSerializers.Length > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple serializers found for Content-Type: {types.contentType}.");
                }

                if (matchingSerializers.Length == 1)
                {
                    return matchingSerializers.First();
                }
            }

            var serializer = matchingSerializers.FirstOrDefault(s => s.EncodingType == types.encodingType);
            if (serializer == null)
            {
                throw new InvalidOperationException(
                    $"Serializer for Content-Type: {contentType} Encoding-Type: {types.encodingType ?? " Not Set"} not registered.");
            }

            return serializer;
        }

        // If the encoding-type is not explicitly specified, determines if the content-type
        // values specifies the encoding.  Example: application/json; charset=utf-8
        private static (string contentType, string encodingType) GetContentTypeAndEncoding(
            string contentType, string encodingType)
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
}
