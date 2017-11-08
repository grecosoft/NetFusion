using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Patterns.Behaviors.Validation
{
    /// <summary>
    /// Domain Behavior responsible for validating its associated domain entity.
    /// </summary>
    public class ValidationBehavior : IValidationBehavior
    {
        private readonly IBehaviorDelegator _entity;

        // Collaborations:
        public IAppContainer AppContainer { get; set; }

        public ValidationBehavior(IBehaviorDelegator entity)
        {
            _entity = entity;
        }

        public ValidationResultSet Validate()
        {
            IObjectValidator validator = AppContainer.CreateValidator(_entity);
            IValidatableType validatable = _entity as IValidatableType;

            // If the base validation has passed, invoke the validation on the
            // entity if supported.
            if (validator.IsValid && validatable != null)
            {
                validatable.Validate(validator);
            }

            return new ValidationResultSet(_entity, validator);
        }
    }
}
