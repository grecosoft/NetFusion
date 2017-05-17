using NetFusion.Common;
using NetFusion.Domain.Entities.Core;

namespace NetFusion.Utilities.Mapping.Behaviors
{
    /// <summary>
    /// Extension methods used to access the mapping behavior for domain entities.
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Determines if the domain entity supports the mapping behavior and 
        /// invokes the behavior to map the entity to the specified target type.
        /// </summary>
        /// <typeparam name="TTarget">The type to map the domain entity.</typeparam>
        /// <param name="domainEntity">The domain entity to be mapped.</param>
        /// <returns>Instance of the resulting mapped object.</returns>
        public static TTarget MapTo<TTarget>(this IEntityDelegator domainEntity)
            where TTarget : class
        {
            Check.NotNull(domainEntity, nameof(domainEntity));

            var behavior = domainEntity.Entity.GetBehavior<IMappingBehavior>();
            if (behavior.supported)
            {
                return behavior.instance.MapTo<TTarget>();
            }
            return null;
        }
    }
}