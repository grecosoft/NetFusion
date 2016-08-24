﻿using NetFusion.Common;
using NetFusion.Messaging;
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

        public string ContentType
        {
            get { return SerializerTypes.Json; }
        }

        public byte[] Serialize(object value)
        {
            Check.NotNull(value, nameof(value));

            var settings = new JsonSerializerSettings {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = _contractResolver
            };

            var json = JsonConvert.SerializeObject(value, settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public object Deserialize(byte[] value, Type vaueType)
        {
            Check.NotNull(value, nameof(value));
            Check.NotNull(vaueType, nameof(vaueType));

            var settings = new JsonSerializerSettings
            {
                ContractResolver = _contractResolver
            };

            var json = Encoding.UTF8.GetString(value);
            return (IMessage)JsonConvert.DeserializeObject(json, vaueType, settings);
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