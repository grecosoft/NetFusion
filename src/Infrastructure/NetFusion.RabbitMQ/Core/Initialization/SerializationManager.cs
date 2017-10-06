using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Serialization;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.RabbitMQ.Core.Initialization
{
    /// <summary>
    /// Encapsulates the logic for serializing messages using the
    /// configured broker serializers.
    /// </summary>
    public class SerializationManager : ISerializationManager
    {
        // Serializer keyed by content-type:
        private IDictionary<string, IBrokerSerializer> _serializers { get; }

        public SerializationManager(IDictionary<string, IBrokerSerializer> serializers)
        {
            Check.NotNull(serializers, nameof(serializers));

            _serializers = serializers;

            // Add the default serializers if not overridden by the application host.
            AddSerializer(serializers, new JsonBrokerSerializer());
            AddSerializer(serializers, new MessagePackBrokerSerializer());

#if NET461
            AddSerializer(serializers, new BinaryBrokerSerializer());
#endif
        }

        public IEnumerable<IBrokerSerializer> Serializers => _serializers.Values;

        private void AddSerializer(
           IDictionary<string, IBrokerSerializer> serializers,
           IBrokerSerializer serializer)
        {
            if (!serializers.Keys.Contains(serializer.ContentType))
            {
                serializers[serializer.ContentType] = serializer;
            }
        }

        // Allows the consumer to pass a list of content-types in priority order
        // and returns the first available content-type.  This is used since
        // the content-type can be specified at different levels.  For example,
        // at the exchange, configuration, or message level.
        private string GetFistContentType(params string[] contentTypes)
        {
            string contentType = contentTypes.FirstOrDefault(ct => ct != null);
            if (contentType == null)
            {
                throw new BrokerException("Serialization type not specified.");
            }
            return contentType;
        }

        public byte[] Serialize(object value, string contentType)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNullOrWhiteSpace(contentType, nameof(contentType));

            IBrokerSerializer serializer = GetSerializer(contentType);
            return serializer.Serialize(value);
        }

        public byte[] Serialize(IMessage message, params string[] contentTypes)
        {
            Check.NotNull(message, nameof(message));

            string contentType = GetFistContentType(contentTypes);
            message.SetContentType(contentType);

            IBrokerSerializer serializer = GetSerializer(contentType);
            return serializer.Serialize(message);
        }

        public IMessage Deserialize(Type messageType, BasicDeliverEventArgs deliveryEvent)
        {
            Check.NotNull(messageType, nameof(messageType));
            Check.NotNull(deliveryEvent, nameof(deliveryEvent));

            string contentType = deliveryEvent.BasicProperties.ContentType;
            if (contentType.IsNullOrWhiteSpace())
            {
                throw new BrokerException(
                    $"The content type for a message of type: {messageType} was not " +
                    $"specified as a basic property.");
            }

            IBrokerSerializer serializer = GetSerializer(contentType);
            return serializer.Deserialize<IMessage>(deliveryEvent.Body, messageType);
        }

        public object Deserialize(string contentType, Type valueType, byte[] value)
        {
            Check.NotNullOrWhiteSpace(contentType, nameof(contentType));
            Check.NotNull(valueType, nameof(valueType));
            Check.NotNull(value, nameof(value));

            IBrokerSerializer serializer = GetSerializer(contentType);
            return serializer.Deserialize(value, valueType);
        }

        public T Deserialize<T>(string contentType, byte[] value)
        {
            Check.NotNullOrWhiteSpace(contentType, nameof(contentType));
            Check.NotNull(value, nameof(value));

            IBrokerSerializer serializer = GetSerializer(contentType);
            return serializer.Deserialize<T>(value, typeof(T));

        }

        private IBrokerSerializer GetSerializer(string contentType)
        {
            IBrokerSerializer serializer = null;

            if (!_serializers.TryGetValue(contentType, out serializer))
            {
                throw new BrokerException($"Serializer for Content Type: {contentType} not registered.");
            }

            return serializer;
        }
    }
}
