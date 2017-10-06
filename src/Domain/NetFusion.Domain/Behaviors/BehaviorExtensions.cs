using NetFusion.Common;
using NetFusion.Domain.Entities.Core;
using NetFusion.Utilities.Validation.Results;

namespace NetFusion.Domain.Behaviors
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
            where TTarget : class, new()
        {
            Check.NotNull(domainEntity, nameof(domainEntity));

            var behavior = domainEntity.Entity.GetBehavior<IMappingBehavior>();
            if (behavior.supported)
            {
                return behavior.instance.MapTo<TTarget>();
            }
            return null;
        }

        /// <summary>
        /// Determines if the specified entity supports the validation behavior.
        /// If supported, the validation method is executed.
        /// </summary>
        /// <param name="domainEntity">The entity to be validated.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResult Validate(this IEntityDelegator domainEntity)
        {
            Check.NotNull(domainEntity, nameof(domainEntity));

            var behavior = domainEntity.Entity.GetBehavior<IValidationBehavior>();
            if (behavior.supported)
            {
                return behavior.instance.Validate();
            }
            return ValidationResult.NotSpecified(domainEntity);
        }
    }
}