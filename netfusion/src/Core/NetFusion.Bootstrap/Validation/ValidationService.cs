using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Container;
using System;

namespace NetFusion.Bootstrap.Validation
{
    /// <summary>
    /// Service that delegates to the IAppContainer to create an instance 
    /// of the host provided IObjectValidator instance used for validation.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly ICompositeContainer _compositeContainer;

        public ValidationService(ICompositeContainer compositeContainer)
        {
            _compositeContainer = compositeContainer ?? throw new ArgumentNullException(nameof(compositeContainer));
        }

        public ValidationResultSet Validate(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), 
                "Object to validate cannot be null.");

            IObjectValidator validator = _compositeContainer.CreateValidator(obj);
            return validator.Validate();
        }
    }
}
