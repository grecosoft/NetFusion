using System;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Serialization
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
        private readonly IDictionary<string, IMessageSerializer> _serializers;

        public SerializationManager()
        {
            _serializers = new Dictionary<string, IMessageSerializer>();

            AddSerializer(new JsonMessageSerializer());
            AddSerializer(new MessagePackSerializer());
        }

        public IEnumerable<IMessageSerializer> Serializers => _serializers.Values;

        public void ClearSerializers() => _serializers.Clear();

        public void AddSerializer(IMessageSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            _serializers[serializer.ContentType] = serializer;
        }

        public byte[] Serialize(object value, string contentType)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Content type name must be specified.", nameof(contentType));

            IMessageSerializer serializer = GetSerializer(contentType);
            return serializer.Serialize(value);
        }

        public object Deserialize(string contentType, Type valueType, byte[] value)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Broker name must be specified.", nameof(contentType));

            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            if (value == null) throw new ArgumentNullException(nameof(value));

            IMessageSerializer serializer = GetSerializer(contentType);
            return serializer.Deserialize(value, valueType);
        }

        public T Deserialize<T>(string contentType, byte[] value)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("Broker name must be specified.", nameof(contentType));

            if (value == null) throw new ArgumentNullException(nameof(value));

            IMessageSerializer serializer = GetSerializer(contentType);
            return serializer.Deserialize<T>(value, typeof(T));
        }

        private IMessageSerializer GetSerializer(string contentType)
        {
            if (!_serializers.TryGetValue(contentType, out IMessageSerializer serializer))
            {
                throw new InvalidOperationException($"Serializer for Content Type: {contentType} not registered.");
            }

            return serializer;
        }
    }
}
