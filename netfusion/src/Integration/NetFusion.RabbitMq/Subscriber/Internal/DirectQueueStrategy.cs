using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a direct exchange.  Sets the default
    /// conventions used for a direct exchange and created queues.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-direct
    /// </summary>
    internal class DirectQueueStrategy : IQueueStrategy
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.Define(attribute.BusName, attribute.ExchangeName, ExchangeType.Direct,
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

        public Task OnMessageReceivedAsync(ConsumeContext context)
        {
            var message = context.DeserializeIntoMessage();
            context.LogReceivedMessage(message);
            
            return context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                context.Subscriber.DispatchInfo, 
                message);
        }
    }
}