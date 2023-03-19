using System.Diagnostics.CodeAnalysis;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.Bus;

public static class MessageExtensions
{
    public static bool TryParseReplyTo(this IMessage message, 
        [MaybeNullWhen(false)]out string busName, [MaybeNullWhen(false)]out string queueName)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        return TryParseReplyTo(message.GetReplyTo(), out busName, out queueName);
    }
    
    public static bool TryParseReplyTo(string? replyTo, 
        [MaybeNullWhen(false)]out string busName, [MaybeNullWhen(false)]out string queueName)
    {
        var parts = replyTo?.Split(":");
        if (parts is not { Length: 2 })
        {
            busName = queueName = null;
            return false;
        }

        busName = parts[0]; 
        queueName = parts[1];
        return true;
    }
}