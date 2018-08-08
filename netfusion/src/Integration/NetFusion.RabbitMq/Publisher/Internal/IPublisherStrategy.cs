using IMessage = NetFusion.Messaging.Types.IMessage;
using System.Threading.Tasks;
using System.Threading;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Implemented by components that vary the message publishing logic based
    /// on the style of message.
    /// </summary>
    internal interface IPublisherStrategy
    {
        /// <summary>
        /// When called, should determine how a message should be published based
        /// on the context of the call and the associated exchange definition.
        /// </summary>
        /// <param name="context">Reference to common services.</param>
        /// <param name="createdExchange">Information about the exchange to which
        /// the message is being published.</param>
        /// <param name="message">The message being published.</param>
        /// <param name="cancellationToken">Task cancellation token.</param>
        /// <returns>Task that will be completed after message is published.</returns>
        Task Publish(IPublisherContext context, CreatedExchange createdExchange, 
            IMessage message,
            CancellationToken cancellationToken);
    }
}