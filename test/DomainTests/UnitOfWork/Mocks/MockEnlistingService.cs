using NetFusion.Domain.Entities;
using NetFusion.Domain.Patterns.Behaviors.Integration;
using NetFusion.Domain.Patterns.UnitOfWork;
using NetFusion.Messaging;
using System;
using System.Threading.Tasks;

namespace DomainTests.UnitOfWork.Mocks
{
    // Example of another application service consuming an integration event.
    public class MockEnlistingService : IMessageConsumer
    {
        private readonly IAggregateUnitOfWork _uow;
        private readonly IDomainEntityFactory _entityFactory;
        public bool ReceivedIntegrationEvent { get; private set; } = false;

        public MockEnlistingService(IAggregateUnitOfWork uow, IDomainEntityFactory entityFactory)
        {
            _uow = uow;
            _entityFactory = entityFactory;
        }

        [InProcessHandler]
        public Task When(MockIntegrationEvent evt)
        {
            ReceivedIntegrationEvent = true;

            if (evt.EnlistingSrvThrowEx)
            {
                throw new InvalidOperationException(
                    "Enlisting Service Exception");
            }

            var aggregate = _entityFactory.Create<MockAggregateTwo>();
            if (evt.EnlistDuplicateIntegrationEvent)
            {
                aggregate.IntegrationEvent(new MockIntegrationEvent());

            }

            if (evt.IsEnlistedAggregateInvalid)
            {
                aggregate.MakeInvalid();
            }

            return _uow.EnlistAsync(aggregate);
        }
    }
}
