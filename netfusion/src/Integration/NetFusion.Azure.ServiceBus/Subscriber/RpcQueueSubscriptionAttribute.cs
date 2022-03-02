using System;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Attribute used to decorate a command handler invoked for a RPC style
    /// of command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RpcQueueSubscriptionAttribute : SubscriptionAttribute
    {
        /// <summary>
        /// Multiple command types can be sent to the same queue.  The message namespace
        /// is used to identify the message on the shared queue.  If not specified here,
        /// can also be specified on the command using the MessageNamespaceAttribute.
        /// </summary>
        public string MessageNamespace { get; set; }

        /// <summary>
        /// Binds command handler to a specific queue defined within an Azure
        /// Service Bus namespace.
        /// </summary>
        /// <param name="namespaceName">The defined namespace containing the queue.</param>
        /// <param name="queueName">The name of the queue defined within the namespace.</param>
        public RpcQueueSubscriptionAttribute(string namespaceName, string queueName)
            : base(namespaceName, queueName)
        {

        }
    }
}