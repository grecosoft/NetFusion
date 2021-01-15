using System;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RpcQueueSubscriptionAttribute : SubscriptionAttribute
    {
        public string MessageNamespace { get; set; }
        
        public RpcQueueSubscriptionAttribute(string namespaceName, string queueName)
            : base(namespaceName, queueName)
        {

        }
    }
}