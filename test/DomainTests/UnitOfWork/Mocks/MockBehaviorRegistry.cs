using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Entities.Registration;
using NetFusion.Domain.Patterns.Behaviors.Integration;
using NetFusion.Domain.Patterns.Behaviors.Validation;

namespace DomainTests.UnitOfWork.Mocks
{
    public class MockBehaviorRegistry : IBehaviorRegistry
    {
        public void Register(IFactoryRegistry registry)
        {
            registry.AddBehavior<IValidationBehavior, ValidationBehavior>();
            registry.AddBehavior<IEventIntegrationBehavior, EventIntegrationBehavior>();
        }
    }
}
