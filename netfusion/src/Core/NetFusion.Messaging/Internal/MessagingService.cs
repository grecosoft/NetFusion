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
            CancellationToken cancellationToken = default, 
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.PublishAsync(domainEvent, cancellationToken, integrationType);
        }

        public Task PublishAsync(IEventSource eventSource, 
            CancellationToken cancellationToken = default, 
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.PublishAsync(eventSource, cancellationToken, integrationType);
        }

        public Task SendAsync(ICommand command, 
            CancellationToken cancellationToken = default,
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.SendAsync(command, cancellationToken, integrationType);
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, 
            CancellationToken cancellationToken = default,
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.SendAsync(command, cancellationToken, integrationType);
        }

        public Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query,
            CancellationToken cancellationToken = default)
        {
            return _queryDispatcher.Dispatch(query, cancellationToken);
        }
    }
}
