using NetFusion.Common;
using NetFusion.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializes a value to JSON representation.  Called by the
    /// MessageBroker when serializing and deserializing messages
    /// to queues.
    /// </summary>
    public class JsonBrokerSerializer : IBrokerSerializer
    {
        private IContractResolver _contractResolver = new CustomContractResolver();

        public string ContentType => SerializerTypes.Json;

        public byte[] Serialize(object value)
        {
            Check.NotNull(value, nameof(value));

            var settings = new JsonSerializerSettings {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = _contractResolver
            };

            string json = JsonConvert.SerializeObject(value, settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public object Deserialize(byte[] value, Type valueType)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(valueType, nameof(valueType));

            if (!valueType.HasDefaultConstructor())
            {
                throw new InvalidOperationException(
                    $"The message type of: {valueType} does not have a default constructor." +
                    $"Required for serialization content-type: {this.ContentType}.");
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = _contractResolver
            };

            string json = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject(json, valueType, settings);
        }

        public T Deserialize<T>(byte[] value, Type valueType)
        {
            return (T)Deserialize(value, valueType);
        }

        private class CustomContractResolver : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var membersToSerialize = base.GetSerializableMembers(objectType)
                    .Concat(objectType.GetProperties().Where(p => p.GetSetMethod() != null))
                    .GroupBy(p => p.Name)
                    .Select(g => g.First())
                    .ToList();

                return membersToSerialize;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (!prop.Writable)
                {
                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        var hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }

                return prop;
            }   
        }
    }
}
