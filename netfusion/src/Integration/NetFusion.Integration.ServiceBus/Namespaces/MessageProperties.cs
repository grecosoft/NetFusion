using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus;
using NetFusion.Integration.Bus;

namespace NetFusion.Integration.ServiceBus.Namespaces;

/// <summary>
/// Reading and parsing of message properties.
/// </summary>
internal static class MessageProperties
{
    public static bool TryParseReplyTo(ProcessMessageEventArgs args, 
        [MaybeNullWhen(false)]out string busName, [MaybeNullWhen(false)]out string queueName)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        return MessageExtensions.TryParseReplyTo(args.Message.ReplyTo, out busName, out queueName);
    }
}