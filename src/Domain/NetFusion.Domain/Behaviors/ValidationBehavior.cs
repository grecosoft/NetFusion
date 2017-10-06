using NetFusion.Domain.Entities.Core;
using NetFusion.Utilities.Validation;
using NetFusion.Utilities.Validation.Core;
using NetFusion.Utilities.Validation.Results;

namespace NetFusion.Domain.Behaviors
{
    /// <summary>
    /// Domain Behavior responsible for validating its associated domain entity.
    /// </summary>
    public class ValidationBehavior : IValidationBehavior
    {
        // Collaborations:
        public IValidationModule ValidationModule { get; set; }

        private IEntityDelegator Entity { get; set; }

        public ValidationBehavior(IEntityDelegator entity)
        {
            this.Entity = entity;
        }

        public ValidationResult Validate()
        {
            IObjectValidator validator = ValidationModule.CreateValidator(this.Entity);
            IValidatableType validatable = this.Entity as IValidatableType;

            if (validator.IsValid && validatable != null)
            {
                validatable.Validate(validator);
            }

            return new ValidationResult(this.Entity, validator);
        }
    }
}
