using System.Text;
using EasyNetQ;

namespace NetFusion.Integration.RabbitMQ.Rpc.Strategies;

/// <summary>
/// Extensions for setting message properties.
/// </summary>
internal static class MessagePropertyExtensions
{
    public const string RpcReplyBusName = "Rpc-reply-bus-name";
    public const string RpcActionNamespace = "Rpc-action-namespace";
    public const string RpcHeaderExceptionIndicator = "Rpc-exception-response";

    /// <summary>
    /// When sending a RPC style message, a string value indicating the action is used
    /// by the consumer to determine how the command should be processed.
    /// </summary>
    /// <param name="messageProps">The message properties being sent with the published message.</param>
    /// <returns>The namespace associated with message.</returns>
    public static string? GetRpcActionNamespace(this MessageProperties messageProps)
    {
        if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

        var headers = messageProps.Headers ?? new Dictionary<string, object>();
        if (! headers.TryGetValue(RpcActionNamespace, out object? value))
        {
            return null;
        }

        byte[] byteValue = (byte[])value;
        return Encoding.UTF8.GetString(byteValue);
    }
    
    /// <summary>
    /// Sets the string value identifying the action namespace associated with the message.  This namespace is
    /// used by the subscriber to determine how the command should be processed.
    /// </summary>
    /// <param name="messageProps">The message properties being sent with the published message.</param>
    /// <param name="actionNamespace">The namespace associated with message.</param>
    public static void SetRpcActionNamespace(this MessageProperties messageProps, string actionNamespace)
    {
        if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

        if (string.IsNullOrWhiteSpace(actionNamespace))
            throw new ArgumentException("Action name not specified.", nameof(actionNamespace));

        messageProps.Headers ??= new Dictionary<string, object>();
        messageProps.Headers[RpcActionNamespace] = actionNamespace;
    }

    /// <summary>
    /// Indicates if the header value indicating a RPC reply exception is set.
    /// </summary>
    /// <param name="messageProps">The received message properties.</param>
    /// <returns>True if RPC exception, otherwise False.</returns>
    public static string? GetRpcReplyException(this MessageProperties messageProps)
    {
        if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

        var headers = messageProps.Headers ?? new Dictionary<string, object>();
        if (headers.TryGetValue(RpcHeaderExceptionIndicator, out object? value))
        {
            return (string)value;
        }
        return null;
    }

    /// <summary>
    /// Indicates if the header value indicating a RPC reply exception is set.
    /// </summary>
    /// <param name="messageProps">The message properties being sent with the published message.</param>
    /// <param name="value">True if the message body is an serialized exception.</param>
    public static void SetRpcReplyException(this MessageProperties messageProps, string value)
    {
        if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

        messageProps.Headers ??= new Dictionary<string, object>();
        messageProps.Headers[RpcHeaderExceptionIndicator] = value;
    }
}