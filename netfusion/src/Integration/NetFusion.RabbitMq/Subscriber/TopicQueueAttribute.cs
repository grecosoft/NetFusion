using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a message handler should be bound to a queue
    /// on a topic exchange.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-topic
    /// </summary>
    public class TopicQueueAttribute : SubscriberQueueAttribute
    {
        public TopicQueueAttribute(string busName, string queueName, string exchangeName, 
            params string[] routeKeys) 
            
            : base(busName, queueName, new TopicQueueFactory())
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new System.ArgumentException("Exchange name not specified.", nameof(exchangeName));

            ExchangeName = exchangeName;
            RouteKeys = routeKeys;
        }
    }
}