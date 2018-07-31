using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a topic exchange.  Sets the default conventions 
    /// used for a topic exchange and created queues.
    /// </summary>
    internal class TopicQueueFactory : IQueueFactory
    {
        public void SetQueueDefaults(QueueDefinition definition)
        {
            definition.IsPassive = false;
            definition.IsDurable = true;
            definition.IsExclusive = false;
            definition.IsAutoDelete = false;

            // Append the unique Plugin Id associated with the hosting application
            // to the queue name.  This will make the name of the queue unique to
            // the application.
            definition.AppendHostId = true;
        }

        public void SetExchangeDefaults(QueueExchangeDefinition definition)
        {
            definition.ExchangeType = ExchangeType.Topic;
            definition.IsPassive = false;
            definition.IsDurable = true;
            definition.IsAutoDelete = false;
        }

        public IQueue CreateQueue(QueueContext context)
        {           
            IQueue queue = context.CreateQueue();

            // Bind the queue to the exchange for each route-key.
            foreach (string routeKey in context.Definition.RouteKeys)
            {
                context.Bus.Advanced.Bind(context.Exchange, queue, routeKey);
            }

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