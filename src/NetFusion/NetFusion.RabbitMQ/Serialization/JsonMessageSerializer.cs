using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using NetFusion.Messaging;
using NetFusion.Common;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Reflection;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializes a message to JSON representation.
    /// </summary>
    public class JsonEventMessageSerializer : IMessageSerializer
    {
        private IContractResolver _contractResolver = new CustomContractResolver();

        public string ContentType
        {
            get { return "application/json; charset=utf-8"; }
        }

        public byte[] Serialize(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            var settings = new JsonSerializerSettings {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = _contractResolver
            };

            var json = JsonConvert.SerializeObject(message, settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public IMessage Deserialize(byte[] message, Type messageType)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(messageType, nameof(messageType));

            var settings = new JsonSerializerSettings
            {
                ContractResolver = _contractResolver
            };

            var json = Encoding.UTF8.GetString(message);
            return (IMessage)JsonConvert.DeserializeObject(json, messageType, settings);
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
