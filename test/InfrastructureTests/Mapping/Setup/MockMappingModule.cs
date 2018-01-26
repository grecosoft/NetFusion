using NetFusion.Mapping.Core;
using System;
using System.Linq;

namespace InfrastructureTests.Mapping.Setup
{
    // Simulates the runtime execution of the module that takes place during the bootstrap process.  
    // The actual module automatically finds all defined mapping strategies and creates a cached
    // lookup of source types to the possible target types to which it can be mapped.  The module
    // all registers the strategies within the DI container so the strategies creates for a given
    // lifetime scope can inject any needed services required to implement the mapping. The below
    // creates a lookup based on unit-test provided strategies.
    public class MockMappingModule : IMappingModule
    {
        public MockMappingModule(TargetMap[] targetMappings)
        {
            SourceTypeMappings = targetMappings.ToLookup(tm => tm.SourceType);
        }

        public ILookup<Type, TargetMap> SourceTypeMappings { get; }
    }
}
