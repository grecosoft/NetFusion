using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Publisher.Internal
{
    /// <summary>
    /// Optional behavior supported by an entity-strategy used to implement
    /// any associated custom Service-Bus publishing logic.
    /// </summary>
    public interface IPublishStrategy
    {
        /// <summary>
        /// Called to apply specific logic when a message is published to the entity.
        /// </summary>
        /// <param name="message">The message being published.</param>
        /// <param name="busMessage">The service bus messages created from the original message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Future Task Result.</returns>
        Task SendToEntityAsync(IMessage message, ServiceBusMessage busMessage, CancellationToken cancellationToken);
    }
}