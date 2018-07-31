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
        public RpcQueueAttribute(string queueName, string exchangeName, 
            string actionName) 
            
            : base(queueName, exchangeName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new System.ArgumentException("message", nameof(actionName));

            QueueDefinition.SetFactory(new RpcQueueFactory());
            QueueDefinition.WithRouteKey(actionName);
        }
    }
}