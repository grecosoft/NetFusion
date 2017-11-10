using NetFusion.Base;
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
            if (value == null) throw new ArgumentNullException(nameof(value));

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
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));

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
                    if (member is PropertyInfo property)
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
