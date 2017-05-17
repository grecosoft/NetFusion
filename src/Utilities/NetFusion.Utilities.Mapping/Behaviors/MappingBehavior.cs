using NetFusion.Common;
using NetFusion.Domain.Entities.Core;

namespace NetFusion.Utilities.Mapping.Behaviors
{
    /// <summary>
    /// Behavior that can be associated with a domain entity allowing
    /// it to be mapped to a specified target type.
    /// </summary>
    public class MappingBehavior : IMappingBehavior
    {
        // Collaborations:
        public IObjectMapper Mapper { get; set; }

        private IEntityDelegator Entity { get; set; }

        public MappingBehavior(IEntityDelegator entity)
        {
            Check.NotNull(entity, nameof(entity));
            this.Entity = entity;
        }

        public TTarget MapTo<TTarget>() where TTarget : class
        {
            return Mapper.Map<TTarget>(this.Entity);
        }
    }
}
