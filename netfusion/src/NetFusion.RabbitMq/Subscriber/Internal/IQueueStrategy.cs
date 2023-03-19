using System.Threading.Tasks;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Implementations contain logic for creating and configuring a queue 
    /// based on its type and for processing received messages.
    /// </summary>
    public interface IQueueStrategy
    {
        /// <summary>
        /// Returns and instance of QueueMeta specifying the type of queue
        /// to be created and the associated route-keys based on the attribute
        /// attached to the in-process message handler.
        /// </summary>
        /// <param name="attribute">The attribute specified on a message handler
        /// that is to be bound to a queue.</param>
        /// <returns>Information about the queue to create and subscribe.</returns>
        QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute);

        /// <summary>
        /// Called when a message is received on the queue.
        /// </summary>
        /// <param name="context">The context of the received message.</param>
        Task OnMessageReceivedAsync(ConsumeContext context);
    }
}