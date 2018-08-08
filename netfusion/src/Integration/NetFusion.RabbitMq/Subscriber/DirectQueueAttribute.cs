using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a message handler should be bound to a queue
    /// on a direct exchange.
    /// When to use:  TODO: Doc URL
    /// </summary>
    public class DirectQueueAttribute : SubscriberQueueAttribute
    {
        public DirectQueueAttribute(string busName, string queueName, string exchangeName, 
            params string[] routeKeys) 
            
            : base(busName, queueName, new DirectQueueFactory())
        {
            ExchangeName = exchangeName;
            RouteKeys = routeKeys;
        }
    }
}