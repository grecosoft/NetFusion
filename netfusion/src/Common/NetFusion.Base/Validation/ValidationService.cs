﻿using System;

namespace NetFusion.Base.Validation
{
    /// <summary>
    /// Service that delegates to ICompositeApp to create an instance 
    /// of the host provided IObjectValidator used for validation.
    /// </summary>
    public class ValidationService : IValidationService
    {
        public ValidationResultSet Validate(object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj), 
                "Object to validate cannot be null.");
            
            IObjectValidator validator = new ObjectValidator(obj);
            return validator.Validate();
        }
    }
}
