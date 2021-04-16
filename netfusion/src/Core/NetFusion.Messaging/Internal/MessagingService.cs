using System.Threading;
using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    ///  Central service for executing Commands, Domain-Events, and Queries.
    /// </summary>
    public class MessagingService : IMessagingService
    {
        // Inner classes delegated to for executing specific message types.
        private readonly MessageDispatcher _messageDispatcher;
        private readonly QueryDispatcher _queryDispatcher;

        public MessagingService(
            MessageDispatcher messageDispatcher,
            QueryDispatcher queryDispatcher)
        {
            _messageDispatcher = messageDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        public Task PublishAsync(IDomainEvent domainEvent,
            IntegrationTypes integrationType = IntegrationTypes.All,
            CancellationToken cancellationToken = default)
        {
            return _messageDispatcher.PublishAsync(domainEvent, integrationType, cancellationToken);
        }

        public Task PublishAsync(IEventSource eventSource,
            IntegrationTypes integrationType = IntegrationTypes.All,
            CancellationToken cancellationToken = default)
        {
            return _messageDispatcher.PublishAsync(eventSource, integrationType, cancellationToken);
        }

        public Task SendAsync(ICommand command,
            IntegrationTypes integrationType = IntegrationTypes.All,
            CancellationToken cancellationToken = default)
        {
            return _messageDispatcher.SendAsync(command, integrationType, cancellationToken);
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command,
            IntegrationTypes integrationType = IntegrationTypes.All,
            CancellationToken cancellationToken = default)
        {
            return _messageDispatcher.SendAsync(command, integrationType, cancellationToken);
        }

        public Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query,
            CancellationToken cancellationToken = default)
        {
            return _queryDispatcher.Dispatch(query, cancellationToken);
        }
    }
}
