using System;
using NetFusion.Base.Validation;
using NetFusion.Bootstrap.Validation;

namespace Demo.Infra
{
    public class CustomValidationService : IValidationService
    {
        public ValidationResultSet Validate(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), 
                "Object to validate cannot be null.");
            
            IObjectValidator validator = new CustomObjectValidator(obj);
            return validator.Validate();
        }
    }
}
