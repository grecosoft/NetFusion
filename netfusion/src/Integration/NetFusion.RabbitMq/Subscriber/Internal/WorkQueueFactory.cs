using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating work queues on the default exchange.  Sets the default
    /// conventions used for a work queues queues.</summary>
    internal class WorkQueueFactory : IQueueFactory
    {
        public void SetQueueDefaults(QueueDefinition definition)
        {
            definition.IsPassive = false;
            definition.IsDurable = true;
            definition.IsExclusive = false;
            definition.IsAutoDelete = false;
        }

        public void SetExchangeDefaults(QueueExchangeDefinition definition)
        {
           // A workqueue does not have an associated exchange.
        }

        public IQueue CreateQueue(QueueContext context)
        {
            IQueue queue = context.CreateQueue();
            return queue;
        }

        public Task OnMessageReceived(ConsumeContext context, IMessage message)
        {
            return context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                context.Subscriber.DispatchInfo, 
                message);
        }
    }
}