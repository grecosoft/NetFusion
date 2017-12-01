using NetFusion.Domain.Entities;
using NetFusion.Domain.Patterns.UnitOfWork;
using NetFusion.Messaging;
using System.Threading.Tasks;

namespace DomainTests.UnitOfWork.Mocks
{
    // Test application-service representing the service that will 
    // modify and commit an aggregate.
    public class MockCommitService : IMessageConsumer
    {
        private readonly IAggregateUnitOfWork _uow;
        private readonly IDomainEntityFactory _entityFactory;

        public MockCommitService(IAggregateUnitOfWork uow, IDomainEntityFactory entityFactory)
        {
            _uow = uow;
            _entityFactory = entityFactory;
        }

        // Creates a new aggregate and invokes a method representing a change
        // in the state of the aggregate.  The change is registers an integration
        // domain-event used to communicate the change to other interested services.
        [InProcessHandler]
        public async Task<MockAggregateOne> OnCommand(MockCommand command)
        {
            MockAggregateOne aggregate = _entityFactory.Create<MockAggregateOne>();
            aggregate.TakeAction(command);

            await _uow.CommitAsync(aggregate, () => {
                aggregate.WasUowCommited = true;
                return Task.CompletedTask;
            });

            return aggregate;
        }
    }
}
