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
        private readonly ICompositeApp _compositeApp;

        public ValidationService(ICompositeApp compositeApp)
        {
            _compositeApp = compositeApp ?? throw new ArgumentNullException(nameof(compositeApp));
        }

        public ValidationResultSet Validate(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), 
                "Object to validate cannot be null.");

            IObjectValidator validator = _compositeApp.CreateValidator(obj);
            return validator.Validate();
        }
    }
}
