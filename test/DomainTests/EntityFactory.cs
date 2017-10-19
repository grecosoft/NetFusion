using DomainTests;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Patterns.Behaviors.Integration;
using NetFusion.Domain.Patterns.Behaviors.State;

namespace DomainUnitTests
{
    public static class EntityFactory
    {
        public static IDomainEntityFactory WithIntegration
        {
            get
            {
                var factory = new DomainEntityFactory(new MockResolver());
                factory.AddBehavior<IEventIntegrationBehavior, EventIntegrationBehavior>();

                return factory;
            }
        }

        public static IDomainEntityFactory WithState
        {
            get
            {
                var factory = new DomainEntityFactory(new MockResolver());
                factory.AddBehavior<IAggregateStateBehavior, AggregateStateBehavior>();

                return factory;
            }
        }
    }
}
