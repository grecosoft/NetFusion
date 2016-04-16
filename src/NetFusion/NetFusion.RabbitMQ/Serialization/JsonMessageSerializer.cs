using Newtonsoft.Json;
using System;
using System.Text;
using NetFusion.Messaging;
using NetFusion.Common;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializes a message to JSON representation.
    /// </summary>
    public class JsonEventMessageSerializer : IMessageSerializer
    {
        public string ContentType
        {
            get { return "application/json; charset=utf-8"; }
        }

        public byte[] Serialize(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
            var json = JsonConvert.SerializeObject(message, settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public IMessage Deserialize(byte[] message, Type messageType)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(messageType, nameof(messageType));

            var json = Encoding.UTF8.GetString(message);
            return (IMessage)JsonConvert.DeserializeObject(json, messageType);
        }
    }
}
