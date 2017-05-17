using NetFusion.Common;
using NetFusion.Common.Extensions.Collection;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Utilities.Validation.Results
{
    /// <summary>
    /// Contains the result of validating an object.  The result contains a list of flattened object validations.
    /// Each object validation has the list of associated validation items.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// The root object that was validated.
        /// </summary>
        public object RootObject { get; private set; }

        /// <summary>
        /// List containing an entry for each object that resulted in validations.
        /// </summary>
        public ObjectValidation[] ObjectValidations { get; private set; }

        /// <summary>
        /// The maximum validation level for all associated validations.
        /// </summary>
        public ValidationTypes ValidationType { get; private set; }

        /// <summary>
        /// Creates new instance representing a validation result.
        /// </summary>
        /// <param name="rootObject">The root object that was validated.</param>
        /// <param name="validator">The validator used to validated the object.</param>
        public ValidationResult(object rootObject, IObjectValidator validator)
        {
            Check.NotNull(rootObject, nameof(rootObject));
            Check.NotNull(validator, nameof(validator));

            this.RootObject = rootObject;

            var validations = new List<ObjectValidation>();
            BuildValidationList(validations, validator);

            this.ValidationType = GetMaxValidationType(validations);
            this.ObjectValidations = validations.ToArray();
        }

        private ValidationResult() { }

        /// <summary>
        /// Returns a validation result indicating that validation was not applied.
        /// </summary>
        /// <param name="rootObject">The root object.</param>
        /// <returns></returns>
        public static ValidationResult NotSpecified(object rootObject)
        {
            Check.NotNull(rootObject, nameof(rootObject));

            return new ValidationResult
            {
                RootObject = rootObject,
                ValidationType = ValidationTypes.NotSpecified,
                ObjectValidations = new ObjectValidation[] { }
            };
        }

        /// <summary>
        /// Determines if any of the object validation items are of the specified validation level.
        /// </summary>
        /// <param name="validationType">The validation level to check.</param>
        /// <returns>True of if there is a validation of the specified type.</returns>
        public bool Contains(ValidationTypes validationType)
        {
            return ObjectValidations.Any(v => v.ValidationType == validationType);
        }

        private ValidationTypes GetMaxValidationType(IEnumerable<ObjectValidation> validations)
        {
            if (validations.Empty())
            {
                return ValidationTypes.NotSpecified;
            }

            return validations.OrderByDescending(v => v.ValidationType)
                .First().ValidationType;
        }

        // Recursively processed the validations and builds a flat list of object validations. 
        private void BuildValidationList(List<ObjectValidation> validations,
            IObjectValidator validator)
        {
            var result = new ObjectValidation(
                    validator.Object,
                    validator.Validations.ToArray());

            if (result.Validations.Any())
            {
                validations.Add(result);
            }

            // Process child object validators.
            foreach (IObjectValidator childValidator in validator.Children)
            {
                BuildValidationList(validations, childValidator);
            }
        }

        /// <summary>
        /// Throws and exception of the result contains any error level validations.
        /// </summary>
        /// <param name="notifyClient">Can be used by consuming code to determine if the 
        /// exception is something that should be returned to the client making the call.
        /// </param>
        public void ThrowIfInvalid(bool notifyClient = false)
        {
            if (this.ValidationType == ValidationTypes.Error)
            {
                throw new ValidationResultException(this.RootObject, notifyClient, this);
            }
        }
    }
}
