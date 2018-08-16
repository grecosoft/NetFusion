using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a message handler should be bound to a queue
    /// on a direct exchange.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-direct
    /// </summary>
    public class DirectQueueAttribute : SubscriberQueueAttribute
    {
        public DirectQueueAttribute(string busName, string queueName, string exchangeName, 
            params string[] routeKeys) 
            
            : base(busName, queueName, new DirectQueueFactory())
        {
            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new System.ArgumentException("Exchange name not specified.", nameof(exchangeName));

            ExchangeName = exchangeName;
            RouteKeys = routeKeys;
        }
    }
}