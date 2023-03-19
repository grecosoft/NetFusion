using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    /// <summary>
    /// Used to specify that a message handler should be bound to a queue
    /// on a fanout exchange.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-fanout
    /// </summary>
    public class FanoutQueueAttribute: SubscriberQueueAttribute
    {
        public FanoutQueueAttribute(string busName, string exchangeName)
            : base(busName, exchangeName, new FanoutQueueStrategy())
        {
            ExchangeName = exchangeName;
        }
    }
}