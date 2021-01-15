using System;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Attribute used to specific a message handler method that should
    /// be bound to a Queue can called when a Command is received.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class QueueSubscriptionAttribute : SubscriptionAttribute
    {
        public QueueSubscriptionAttribute(string namespaceName, string queueName)
            : base(namespaceName, queueName)
        {
            
        }
    }
}