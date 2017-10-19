using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;

namespace DomainTests.Behaviors.Mocks
{
    public class Aggregates : IAggregate
    {
        public IBehaviorDelegatee Behaviors { get; private set; }

        void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
        {
            Behaviors = behaviors;
        }
    }

    public class AggregateTwo : IAggregate
    {
        public IBehaviorDelegatee Behaviors { get; private set; }

        void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
        {
            Behaviors = behaviors;
        }
    }
}
