using NetFusion.Domain.Entities;
using NetFusion.Utilities.Validation.Results;

namespace NetFusion.Domain.Behaviors
{
    /// <summary>
    /// Defines the contract to be implemented by a domain behavior
    /// responsible for validating the domain entity.
    /// </summary>
    public interface IValidationBehavior : IDomainBehavior
    {
        /// <summary>
        /// Validates the domain entity and optionally should validate any needed child entities.
        /// </summary>
        /// <returns>The result of the validation.</returns>
        ValidationResult Validate();
    }
}
