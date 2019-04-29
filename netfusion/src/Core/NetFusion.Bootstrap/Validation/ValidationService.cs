using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Container;
using System;
using NetFusion.Bootstrap.Refactors;

namespace NetFusion.Bootstrap.Validation
{
    /// <summary>
    /// Service that delegates to the IAppContainer to create an instance 
    /// of the host provided IObjectValidator instance used for validation.
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly ICompositeAppContainer _appContainer;

        public ValidationService(ICompositeAppContainer appContainer)
        {
            _appContainer = appContainer ?? throw new ArgumentNullException(nameof(appContainer));
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
