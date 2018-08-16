using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a topic exchange.  Sets the default conventions 
    /// used for a topic exchange and created queues.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-topic
    /// </summary>
    internal class TopicQueueFactory : IQueueFactory
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.Define(attribute.BusName, attribute.ExchangeName, ExchangeType.Topic,
                config =>
                {
                    config.IsAutoDelete = false;
                    config.IsDurable = true;
                    config.IsPersistent = true;
                    config.IsPassive = false;
                });
            
            var queue = QueueMeta.Define(attribute.QueueName, exchange,
                config =>
                {
                    config.IsAutoDelete = false;
                    config.IsDurable = true;
                    config.IsPassive = false;
                    config.IsExclusive = false;
                    config.AppendHostId = true;
                    config.RouteKeys = attribute.RouteKeys;
                });

            return queue;
        }

        public Task OnMessageReceived(ConsumeContext context)
        {
            var message = context.DeserializeIntoMessage();
            context.LogReceivedMessage(message);
            
            return context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                context.Subscriber.DispatchInfo, 
                message);
        }
    }
}