using NetFusion.Common;
using NetFusion.Domain.Entities.Core;
using NetFusion.Utilities.Validation.Results;

namespace NetFusion.Utilities.Validation.Behaviors
{
    /// <summary>
    /// Validation specific entity behavior extension methods.
    /// </summary>
    public static class BehaviorExtensions
    {
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
