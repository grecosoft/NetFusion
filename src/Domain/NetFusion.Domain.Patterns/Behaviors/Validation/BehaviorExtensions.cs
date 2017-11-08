using NetFusion.Base.Validation;
using NetFusion.Common;
using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Patterns.Behaviors.Validation
{
    /// <summary>
    /// Extension methods used to access the validation behavior for domain entities.
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Determines if the specified entity supports the validation behavior.
        /// If supported, the validation method is executed.
        /// </summary>
        /// <param name="domainEntity">The entity to be validated.</param>
        /// <returns>The validation result.</returns>
        public static ValidationResultSet Validate(this IBehaviorDelegator domainEntity)
        {
            Check.NotNull(domainEntity, nameof(domainEntity));

            var behavior = domainEntity.Behaviors.Get<IValidationBehavior>();
            if (behavior.supported)
            {
                return behavior.instance.Validate();
            }
            return ValidationResultSet.ValidResult(domainEntity);
        }
    }
}
