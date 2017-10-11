using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Service containing methods to publish messages to all registered
    /// message dispatchers for delivery to their corresponding consumers.
    /// </summary>
    public interface IMessagingService
    {
        /// <summary>
        /// Publishes a command to all associated consumer message handlers.
        /// </summary>
        /// <param name="command">The command to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Future result.</returns>
        Task PublishAsync(ICommand command,
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Publishes a command to all associated consumer message handlers and returns 
        /// the consumer's response.</summary>
        /// <param name="command">The command to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Future result.</returns>
        Task<TResult> PublishAsync<TResult>(ICommand<TResult> command,
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Publishes a domain-event to all associated consumer message handlers.
        /// </summary>
        /// <param name="domainEvent">The event to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Future result.</returns>
        Task PublishAsync(IDomainEvent domainEvent,
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Publishes domain-events associated with an event source.  
        /// </summary>
        /// <param name="eventSource">The event source having associated events.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Future result.</returns>
        Task PublishAsync(IEventSource eventSource,
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);
    }
}
