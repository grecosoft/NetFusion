using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a fanout exchange.  Sets the default conventions 
    /// used for a fanout exchange and created queues.</summary>
    internal class FanoutQueueFactory : IQueueFactory
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.Define(attribute.BusName, attribute.ExchangeName, ExchangeType.Fanout,
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsPassive = false;
                    config.IsPersistent = false;
                });
            
            var queue = QueueMeta.Define(attribute.QueueName, exchange,
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsPassive = false;
                    config.IsExclusive = true;
                    config.AppendUniqueId = true;
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