using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Validation;
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
        public IValidationService ValidationService { get; set; }

        public ValidationBehavior(IBehaviorDelegator entity)
        {
            _entity = entity;
        }

        public ValidationResultSet Validate()
        {
            return ValidationService.Validate(_entity);
        }
    }
}
