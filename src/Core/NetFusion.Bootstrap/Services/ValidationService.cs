using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Container;
using System;

namespace NetFusion.Bootstrap.Validation
{
    /// <summary>
    /// Simple service that delegates to the IAppContainer to create an instance 
    /// of the host provided IObjectValidator instance used to validated an object.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly IAppContainer _appContainer;

        public ValidationService(IAppContainer appContainer)
        {
            _appContainer = appContainer;
        }

        public ValidationResultSet Validate(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), 
                "Object to validate cannot be null.");

            IObjectValidator validator = _appContainer.CreateValidator(obj);
            return validator.Validate();
        }
    }
}
