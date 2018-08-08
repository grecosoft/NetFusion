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
        public string ActionName { get; }
        
        public RpcQueueAttribute(string busName, string queueName, string actionName) 
            
            : base(busName, queueName, new RpcQueueFactory())
        {
            ActionName = actionName;
        }
    }
}