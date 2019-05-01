using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Service containing methods to send commands, dispatch queries, and
    /// publish domain events to their corresponding consumers.
    /// </summary>
    public interface IMessagingService
    {
        /// <summary>
        /// Sends a command to its associated consumer.
        /// </summary>
        /// <param name="command">The command to be sent.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Task result.</returns>
        Task SendAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Sends a command to all associated consumer message handlers and returns 
        /// the consumer's response.</summary>
        /// <param name="command">The command to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Task result.</returns>
        Task<TResult> SendAsync<TResult>(ICommand<TResult> command, 
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Publishes a domain-event to all associated consumer message handlers.
        /// </summary>
        /// <param name="domainEvent">The event to be published.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Task result.</returns>
        Task PublishAsync(IDomainEvent domainEvent,
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Publishes domain-events associated with an event source.  
        /// </summary>
        /// <param name="eventSource">The event source having associated events.</param>
        /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
        /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
        /// <returns>Task result.</returns>
        Task PublishAsync(IEventSource eventSource,
            CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All);

        /// <summary>
        /// Dispatches a query to its corresponding consumer.  When dispatching the
        /// query, it passes through a set of pre/post filters that can alter the
        /// query or consumer returned result.
        /// </summary>
        /// <typeparam name="TResult">The type of the query result.</typeparam>
        /// <param name="query">The query to be dispatched to its corresponding consumer.</param>
        /// <param name="cancellationToken">Optional cancellation token used to cancel the asynchronous task.</param>
        /// <returns>Task containing the result of the consumer.</returns>
        Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query,
           CancellationToken cancellationToken = default(CancellationToken));
    }
}
