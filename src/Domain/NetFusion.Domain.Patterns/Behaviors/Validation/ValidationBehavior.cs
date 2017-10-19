using NetFusion.Domain.Entities.Core;
using NetFusion.Utilities.Validation;
using NetFusion.Utilities.Validation.Core;
using NetFusion.Utilities.Validation.Results;

namespace NetFusion.Domain.Patterns.Behaviors.Validation
{
    /// <summary>
    /// Domain Behavior responsible for validating its associated domain entity.
    /// </summary>
    public class ValidationBehavior : IValidationBehavior
    {
        // Collaborations:
        public IValidationModule ValidationModule { get; set; }

        private readonly IBehaviorDelegator _entity;

        public ValidationBehavior(IBehaviorDelegator entity)
        {
            _entity = entity;
        }

        public ValidationResult Validate()
        {
            IObjectValidator validator = ValidationModule.CreateValidator(_entity);
            IValidatableType validatable = _entity as IValidatableType;

            // If the base validation has passed, invoke the validation on the
            // entity if supported.
            if (validator.IsValid && validatable != null)
            {
                validatable.Validate(validator);
            }

            return new ValidationResult(_entity, validator);
        }
    }
}
