using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a message handler should be bound to a queue
    /// on a topic exchange.
    /// When to use:  TODO: Doc URL
    /// </summary>
    public class TopicQueueAttribute : SubscriberQueueAttribute
    {
        public TopicQueueAttribute(string busName, string queueName, string exchangeName, 
            params string[] routeKeys) 
            
            : base(busName, queueName, new TopicQueueFactory())
        {
            ExchangeName = exchangeName;
            RouteKeys = routeKeys;
        }
    }
}