using NetFusion.Common;
using System.Linq;

namespace NetFusion.Utilities.Validation.Results
{
    /// <summary>
    /// Contains the validation results for a specific object.
    /// </summary>
    public class ObjectValidation
    {
        /// <summary>
        /// The validated object.
        /// </summary>
        public object Object { get; }

        /// <summary>
        /// List of validations associated with the object.
        /// </summary>
        public ValidationItem[] Validations { get; }

        /// <summary>
        /// The maximum record validation level.
        /// </summary>
        public ValidationTypes ValidationType { get; }


        /// <summary>
        /// Creates an object containing validation associated with an object.
        /// </summary>
        /// <param name="obj">The validated object.</param>
        /// <param name="validations">The associated validations.</param>
        public ObjectValidation(object obj, 
            ValidationItem[] validations)
        {
            Check.NotNull(obj, nameof(obj));
            Check.NotNull(validations, nameof(validations));

            this.Object = obj;
            this.Validations = validations;
            this.ValidationType = GetMaxValidationType();
        }

        private ValidationTypes GetMaxValidationType()
        {
            if (Validations.Length == 0)
            {
                return ValidationTypes.NotSpecified;
            }

            return Validations.OrderByDescending(v => v.ValidationType)
                .First().ValidationType;
        }
    }
}
