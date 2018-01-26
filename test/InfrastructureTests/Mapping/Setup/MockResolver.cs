using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Registration;
using NetFusion.Domain.Patterns.Behaviors.Mapping;
using NetFusion.Mapping.Core;

namespace InfrastructureTests.Mapping.Setup
{
    // Mock resolver that correctly configures the MappingBehavior to use the
    // ObjectMapper instance configured for the unit-tests.  The actual resolver
    // used at runtime delegates do a DI container instance.
    public class MockResolver : IDomainServiceResolver
    {
        private TargetMap[] _targetMaps;

        public MockResolver(TargetMap[] targetMaps)
        {
            _targetMaps = targetMaps;
        }

        public void ResolveDomainServices(IDomainBehavior domainBehavior)
        {
            if (domainBehavior is MappingBehavior mappingBehavior)
            {
                mappingBehavior.Mapper = ObjectMapperConfig.CreateObjectMapper(_targetMaps);
            }
        }
    }
}
