using RabbitMQ.Client;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core
{
    public static class BasicPropertiesExtensions
    {
        public static bool IsRpcReplyException(this IBasicProperties basicProps)
        {
            var headers = basicProps.Headers ?? new Dictionary<string, object>();
            object value = null;

            if(headers.TryGetValue("IsRpcReplyException", out value))
            {
                return (bool)value;
            }
            return false;
        }
    }
}
