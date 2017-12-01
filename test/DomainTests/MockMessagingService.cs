using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DomainTests
{
    public class MockMessagingService : IMessagingService
    {
        private List<IDomainEvent> _publishedEvents = new List<IDomainEvent>();

        public static MockMessagingService Mock => new MockMessagingService();

        private MockMessagingService()
        {

        }

        public Task SendAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken), IntegrationTypes integrationType = IntegrationTypes.All)
        {
            throw new System.NotImplementedException();
        }

        public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default(CancellationToken), IntegrationTypes integrationType = IntegrationTypes.All)
        {
            throw new System.NotImplementedException();
        }

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken), IntegrationTypes integrationType = IntegrationTypes.All)
        {
            throw new System.NotImplementedException();
        }

        public Task PublishAsync(IEventSource eventSource, CancellationToken cancellationToken = default(CancellationToken), IntegrationTypes integrationType = IntegrationTypes.All)
        {
            _publishedEvents.AddRange(eventSource.DomainEvents);
            return Task.CompletedTask;
        }
    }
}
