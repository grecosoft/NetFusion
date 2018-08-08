using System;
using System.Collections.Generic;
using System.Text;
using EasyNetQ;

namespace NetFusion.RabbitMQ
{
    internal static class Extensions
    {
        public const string RpcReplyBusName = "Rpc-reply-bus-name";
        public const string RpcActionName = "Rpc-action-name";
        public const string RpcHeaderExceptionIndicator = "Rpc-exception-response";
       
        /// <summary>
        /// A key value used to identify the message bus to which  the consumer should publish the
        /// reply to the command.  The value will be used to look up the IBus instance configuration
        /// corresponding the the name when publishing the reply.
        /// </summary>
        /// <param name="messageProps">The received message properties.</param>
        /// <returns>The value or an exception if not present.</returns>
        public static string GetRpcReplyBusConfigName(this MessageProperties messageProps)
        {
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            var headers = messageProps.Headers ?? new Dictionary<string, object>();
            if (! headers.TryGetValue(RpcReplyBusName, out object value))
            {
                throw new InvalidOperationException($"A message header value named: {RpcReplyBusName} is not present.");
            }

            byte[] byteValue = (byte[])value;
            return Encoding.UTF8.GetString(byteValue);
        }

        /// <summary>
        /// A key value used to identify the message bus to which  the consumer should publish the
        /// reply to the command.  The value will be used to look up the IBus instance configuration
        /// corresponding the the name when publishing the reply.
        /// </summary>
        /// <param name="messageProps">The message properties being sent with the published message.</param>
        /// <param name="busConfigName">The configuration name used to identify the bus.</param>
        public static void SetRpcReplyBusConfigName(this MessageProperties messageProps, string busConfigName)
        {
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            if (string.IsNullOrWhiteSpace(busConfigName))
                throw new ArgumentException("Bus name not specified.", nameof(busConfigName));

            messageProps.Headers = messageProps.Headers ?? new Dictionary<string, object>();
            messageProps.Headers[RpcReplyBusName] = busConfigName;
        }

        public static string GetRpcActionName(this MessageProperties messageProps)
        {
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            var headers = messageProps.Headers ?? new Dictionary<string, object>();
            if (! headers.TryGetValue(RpcActionName, out object value))
            {
                throw new InvalidOperationException($"A message header value named: {RpcActionName} is not present.");
            }

            byte[] byteValue = (byte[])value;
            return Encoding.UTF8.GetString(byteValue);
        }
        
        public static void SetRpcActionName(this MessageProperties messageProps, string actionName)
        {
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentException("Action name not specified.", nameof(actionName));

            messageProps.Headers = messageProps.Headers ?? new Dictionary<string, object>();
            messageProps.Headers[RpcActionName] = actionName;
        }

        /// <summary>
        /// Indicates if the header value indicating a RPC reply exception is set.
        /// </summary>
        /// <param name="messageProps">The received message properties.</param>
        /// <returns>True if RPC exception, otherwise False.</returns>
        public static bool IsRpcReplyException(this MessageProperties messageProps)
        {
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            var headers = messageProps.Headers ?? new Dictionary<string, object>();
            if (headers.TryGetValue(RpcHeaderExceptionIndicator, out object value))
            {
                return (bool)value;
            }
            return false;
        }

        /// <summary>
        /// Indicates if the header value indicating a RPC reply exception is set.
        /// </summary>
        /// <param name="messageProps">The message properties being sent with the published message.</param>
        /// <param name="value">True if the message body is an serialized exception.</param>
        public static void SetRpcReplyException(this MessageProperties messageProps, bool value)
        {
            if (messageProps == null) throw new ArgumentNullException(nameof(messageProps));

            messageProps.Headers = messageProps.Headers ?? new Dictionary<string, object>();
            messageProps.Headers[RpcHeaderExceptionIndicator] = value;
        }
    }
}