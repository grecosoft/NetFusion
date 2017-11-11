using NetFusion.Base.Validation;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Patterns.Behaviors.Integration;

namespace DomainTests.UnitOfWork.Mocks
{
    // Aggregate used for testing integration events with other services.
    public class MockAggregateOne : IAggregate,
        IValidatableType
    {
        public IBehaviorDelegatee Behaviors { get; private set; }
        public bool WasUowCommited { get; set; }
        private MockCommand _testCommand;

        void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
        {
            Behaviors = behaviors;
        }

        public void TakeAction(MockCommand command)
        {
            _testCommand = command;

            var integrationEvt = new MockIntegrationEvent
            {
                EnlistingSrvThrowEx = command.EnlistingSrvThrowEx,
                EnlistDuplicateIntegrationEvent = command.EnlistDuplicateIntegrationEvent,
                IsEnlistedAggregateInvalid = command.IsEnlistedAggregateInvalid

            };

            this.IntegrationEvent(integrationEvt);
        }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(!_testCommand.IsCommittedAggregateInvalid, "Invalidate Aggregate");
        }
    }
}
