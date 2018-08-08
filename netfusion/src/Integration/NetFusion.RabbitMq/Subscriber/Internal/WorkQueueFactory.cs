using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating work queues on the default exchange.  Sets the default
    /// conventions used for a work queues queues.</summary>
    internal class WorkQueueFactory : IQueueFactory
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.DefineDefault(attribute.BusName, attribute.QueueName,
                config =>
                {
                    config.IsAutoDelete = false;
                    config.IsDurable = true;
                    config.IsPassive = false;
                    config.IsExclusive = false;
                });

            return exchange.QueueMeta;
        }

        public Task OnMessageReceived(ConsumeContext context, IMessage message)
        {
            return context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                context.Subscriber.DispatchInfo, 
                message);
        }
    }
}