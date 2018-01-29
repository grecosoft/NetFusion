using NetFusion.Messaging.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    ///  Central service for executing Commands, Domain-Events, and Queries.
    /// </summary>
    public class MessagingService : IMessagingService
    {
        // Inner classes delegated to for executing specific types.
        private Lazy<MessageDispatcher> _messageDispatcher;
        private Lazy<QueryDispatcher> _queryDispatcher;

        public MessagingService(
            Lazy<MessageDispatcher> messageDispatcher,
            Lazy<QueryDispatcher> queryDispatcher)
        {
            _messageDispatcher = messageDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        public Task PublishAsync(IDomainEvent domainEvent, 
            CancellationToken cancellationToken = default(CancellationToken), 
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.Value.PublishAsync(domainEvent, cancellationToken, integrationType);
        }

        public Task PublishAsync(IEventSource eventSource, 
            CancellationToken cancellationToken = default(CancellationToken), 
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.Value.PublishAsync(eventSource, cancellationToken, integrationType);
        }

        public Task SendAsync(ICommand command, 
            CancellationToken cancellationToken = default(CancellationToken), 
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.Value.SendAsync(command, cancellationToken, integrationType);
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, 
            CancellationToken cancellationToken = default(CancellationToken), 
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            return _messageDispatcher.Value.SendAsync(command, cancellationToken, integrationType);
        }

        public Task<TResult> DispatchAsync<TResult>(IQuery<TResult> query,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return _queryDispatcher.Value.Dispatch(query, cancellationToken);
        }
    }
}
