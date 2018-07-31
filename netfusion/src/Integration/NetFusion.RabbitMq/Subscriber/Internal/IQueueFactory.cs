using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Implementations contain logic for creating and configuring a queue 
    /// based on its type.
    /// </summary>
    public interface IQueueFactory
    {
        /// <summary>
        /// Called to populate the passed QueueDefinition with the default
        /// conventions to be used when creating a queue of a specific type. 
        /// </summary>
        /// <param name="definition">Definition to initialize.</param>
        void SetQueueDefaults(QueueDefinition definition);
        
        /// <summary>
        /// Called to populate the passed QueueExchangeDefinition with the default
        /// conventions to be used when creating an exchange of a specific type. 
        /// </summary>
        /// <param name="definition">Defintion to initialize.</param>
        void SetExchangeDefaults(QueueExchangeDefinition definition);

        /// <summary>
        /// Instructs the factory to create a queue based on the details
        /// specified within the passed context.
        /// </summary>
        /// <param name="context">The context under which the queue is to
        /// be created.</param>
        /// <returns>Instance of the created queue.</returns>
        IQueue CreateQueue(QueueContext context);

        /// <summary>
        /// Called when a message is received on the queue.
        /// </summary>
        /// <param name="context">The context of the received message.</param>
        /// <param name="message">The received message.</param>
        Task OnMessageReceived(ConsumeContext context, IMessage message);

    }
}