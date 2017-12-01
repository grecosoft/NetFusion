using NetFusion.Base.Validation;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;

namespace DomainTests.UnitOfWork.Mocks
{
    public class MockAggregateTwo : IAggregate,
            IValidatableType
    {
        public IBehaviorDelegatee Behaviors { get; private set; }
        private bool _makeInvalid { get; set; }

        public void MakeInvalid()
        {
            _makeInvalid = true;
        }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(!_makeInvalid, "Invalidate Aggregate");
        }

        void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
        {
            Behaviors = behaviors;
        }
    }
}
