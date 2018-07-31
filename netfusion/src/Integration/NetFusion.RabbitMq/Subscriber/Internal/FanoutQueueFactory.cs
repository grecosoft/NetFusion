using System.Threading.Tasks;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a fanout exchange.  Sets the default conventions 
    /// used for a fanout exchange and created queues.</summary>
    internal class FanoutQueueFactory : IQueueFactory
    {
        public void SetQueueDefaults(QueueDefinition definition)
        {
            definition.IsPassive = false;
            definition.IsDurable = false;
            definition.IsExclusive = true;
            definition.IsAutoDelete = true;

            // Append a unique Id to the base queue name.  The Plugin Id of the host
            // is not used since if multiple instances of the host are running, all
            // instances should receive the message (i.e. Not Round-Robin)
            definition.AppendUniqueId = true;
        }

        public void SetExchangeDefaults(QueueExchangeDefinition definition)
        {
            definition.ExchangeType = ExchangeType.Fanout;
            definition.IsPassive = false;
            definition.IsDurable = false;
            definition.IsAutoDelete = true;
        }

        public IQueue CreateQueue(QueueContext context)
        {
            IQueue queue = context.CreateQueue();
            context.Bus.Advanced.Bind(context.Exchange, queue, string.Empty);

            return queue;
        }

        public Task OnMessageReceived(ConsumeContext context, Messaging.Types.IMessage message)
        {
            return context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                context.Subscriber.DispatchInfo, 
                message);
        }
    }
}