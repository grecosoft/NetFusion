using NetFusion.Domain.Entities.Core;
using NetFusion.Mapping;
using System;

namespace NetFusion.Domain.Patterns.Behaviors.Mapping
{
    /// <summary>
    /// Behavior that can be associated with a domain entity allowing
    /// it to be mapped to a specified target type.
    /// </summary>
    public class MappingBehavior : IMappingBehavior
    {
        // Collaborations:
        public IObjectMapper Mapper { get; set; }

        private IBehaviorDelegator Entity { get; set; }

        public MappingBehavior(IBehaviorDelegator entity)
        {
            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        public TTarget MapTo<TTarget>() where TTarget : class, new()
        {
            return Mapper.Map<TTarget>(Entity);
        }
    }
}
