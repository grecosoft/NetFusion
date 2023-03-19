using System.Diagnostics.CodeAnalysis;
using EasyNetQ;
using NetFusion.Integration.Bus;

namespace NetFusion.Integration.RabbitMQ.Bus;

public static class MessagePropertiesExtensions
{
    /// <summary>
    /// Parses the reply queue name and the name of the bus on which it resides. 
    /// </summary>
    /// <param name="messageProperties">The received message properties.</param>
    /// <param name="busName">The name of the bus on which the reply queue is located.</param>
    /// <param name="queueName">The name of the reply queue.</param>
    /// <returns>True if the replay queue could be determined.  Otherwise, false.</returns>
    public static bool TryParseReplyTo(this MessageProperties messageProperties,
        [MaybeNullWhen(false)]out string busName, [MaybeNullWhen(false)] out string queueName)
    {
        if (messageProperties == null) throw new ArgumentNullException(nameof(messageProperties));
        return MessageExtensions.TryParseReplyTo(messageProperties.ReplyTo, out busName, out queueName);
    }

    public static void SetPublishOptions(this MessageProperties messageProperties, 
        string contentType,
        PublishOptions publishOptions)
    {
        messageProperties.ContentType = contentType;
        messageProperties.DeliveryMode = Convert.ToByte(publishOptions.IsPersistent ? 2 : 1);

        if (publishOptions.Priority != null)
        {
            messageProperties.Priority = publishOptions.Priority.Value;
        }
    }
}