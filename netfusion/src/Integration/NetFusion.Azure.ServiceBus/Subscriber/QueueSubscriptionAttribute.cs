using System;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Attribute used to specific a message handler method that should
    /// be bound to a Queue and called when a Command is received.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class QueueSubscriptionAttribute : SubscriptionAttribute
    {
        /// <summary>
        /// Binds command handler to a specific queue defined within an Azure
        /// Service Bus namespace.
        /// </summary>
        /// <param name="namespaceName">The defined namespace containing the queue.</param>
        /// <param name="queueName">The name of the queue defined within the namespace.</param>
        public QueueSubscriptionAttribute(string namespaceName, string queueName)
            : base(namespaceName, queueName)
        {
            
        }
    }
}