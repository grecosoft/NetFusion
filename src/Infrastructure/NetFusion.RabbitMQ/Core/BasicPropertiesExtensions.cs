using NetFusion.RabbitMQ.Core.Rpc;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// BasicProperties extensions.
    /// </summary>
    public static class BasicPropertiesExtensions
    {
        /// <summary>
        /// Determines if the header value indicating a RPC reply exception is set.
        /// </summary>
        /// <param name="basicProps">The properties to check.</param>
        /// <returns>True if RPC exception, otherwise False.</returns>
        public static bool IsRpcReplyException(this IBasicProperties basicProps)
        {
            var headers = basicProps.Headers ?? new Dictionary<string, object>();

            if (headers.TryGetValue(RpcClient.RPC_HEADER_EXCEPTION_INDICATOR, out object value))
            {
                return (bool)value;
            }
            return false;
        }

        public static string GetBrokerName(this IBasicProperties basicProps)
        {
            var headers = basicProps.Headers ?? new Dictionary<string, object>();

            if (headers.TryGetValue(RpcClient.RPC_BROKER_NAME, out object value))
            {
                byte[] byteValue = (byte[])value;
                return Encoding.UTF8.GetString(byteValue);
            }

            throw new BrokerException("The RPC broker name header is not present.");
        }
    }
}
