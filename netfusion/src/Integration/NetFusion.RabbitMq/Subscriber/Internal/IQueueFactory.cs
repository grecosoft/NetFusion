using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Implementations contain logic for creating and configuring a queue 
    /// based on its type.
    /// </summary>
    public interface IQueueFactory
    {
        QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute);

        /// <summary>
        /// Called when a message is received on the queue.
        /// </summary>
        /// <param name="context">The context of the received message.</param>
        /// <param name="message">The received message.</param>
        Task OnMessageReceived(ConsumeContext context, IMessage message);

    }
}