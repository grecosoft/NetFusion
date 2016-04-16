using System;
using NetFusion.Messaging;
using NetFusion.Common.Serialization;
using NetFusion.Common;

namespace NetFusion.RabbitMQ.Serialization
{
    /// <summary>
    /// Serializes a message to a binary representation.
    /// </summary>
    public class BinaryMessageSerializer : IMessageSerializer
    {
        public string ContentType
        {
            get { return "application/octet-stream"; }
        }

        public byte[] Serialize(IMessage message)
        {
            Check.NotNull(message, nameof(message));
            return BinaryFormatterUtility.Serialize(message);
        }

        public IMessage Deserialize(byte[] message, Type messageType)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(messageType, nameof(messageType));

            return BinaryFormatterUtility.Deserialize<IMessage>(message);
        }
    }
}
