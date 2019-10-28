using System;
using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Subscribes a message handler that is invoked when a RPC style
    /// command is received with a route-key equal to the specified
    /// action-name value.
    /// </summary>
    public class RpcQueueAttribute : SubscriberQueueAttribute
    {
        public string ActionNamespace { get; }
        
        public RpcQueueAttribute(string busName, string queueName, string actionNamespace) 
            
            : base(busName, queueName, new RpcQueueStrategy())
        {
            if (string.IsNullOrWhiteSpace(actionNamespace))
                throw new ArgumentException("Action namespace not specified", nameof(actionNamespace));

            ActionNamespace = actionNamespace;
        }
    }
}