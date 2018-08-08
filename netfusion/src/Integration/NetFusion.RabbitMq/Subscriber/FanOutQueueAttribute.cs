using NetFusion.RabbitMQ.Subscriber.Internal;

namespace NetFusion.RabbitMQ.Subscriber
{
    public class FanoutQueueAttribute: SubscriberQueueAttribute
    {
        public FanoutQueueAttribute(string busName, string exchangeName) 
            
            : base(busName, exchangeName, new FanoutQueueFactory())
        {
            ExchangeName = exchangeName;
        }
    }
}