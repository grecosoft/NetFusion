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
    /// A consuming application can override these default serializers by instanciating this 
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

            IMessageSerializer existingSerializer = GetSerializer(serializer.ContentType, serializer.EncodingType);
            if (existingSerializer != null)
            {
                throw new InvalidOperationException(
                    $"Serializer already exists Content-Type: {serializer.ContentType} " + 
                    $"Encoding-Type: {serializer.EncodingType ?? ""}.");
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

        private IMessageSerializer GetSerializer(string contentType, string encodingType)
        {
            var matchingSerializers = _serializers.Where(s => s.ContentType == contentType).ToArray();
            if (string.IsNullOrEmpty(encodingType))
            {
                if (matchingSerializers.Length > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple serializers found for Content-Type: {contentType}.");
                }

                if (matchingSerializers.Length == 1)
                {
                    return matchingSerializers.First();
                }
            }

            var serializer = matchingSerializers.FirstOrDefault(s => s.EncodingType == encodingType);
            if (serializer == null)
            {
                throw new InvalidOperationException(
                    $"Serializer for Content-Type: {contentType} Encoding-Type: {encodingType} not registered.");
            }

            return serializer;
        }
    }
}
