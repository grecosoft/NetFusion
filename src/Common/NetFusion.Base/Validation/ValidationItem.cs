using System.Collections.Generic;

namespace NetFusion.Base.Validation
{
    /// <summary>
    /// Class recording a validation result for an object's property.
    /// </summary>
    public class ValidationItem
    {
        /// <summary>
        /// The associated message description.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The name of the object properties pertaining to the validation.
        /// </summary>
        public IEnumerable<string> PropertyNames { get; }

        /// <summary>
        /// The validation level.
        /// </summary>
        public ValidationTypes ValidationType { get; }

        /// <summary>
        /// Creates an object property validation.
        /// </summary>
        /// <param name="message">The message associated with the validation.</param>
        /// <param name="propertyNames">The object properties pertaining to the validation.</param>
        /// <param name="validationType">The validation level.</param>
        public ValidationItem(
            string message,
            IEnumerable<string> propertyNames,
            ValidationTypes validationType)
        {
            Message = message;
            PropertyNames = propertyNames;
            ValidationType = validationType;
        }

        /// <summary>
        /// Creates an object validation.
        /// </summary>
        /// <param name="message">The message associated with the validation.</param>
        /// <param name="propertyNames">The object properties pertaining to the validation.</param>
        public ValidationItem(
            string message,
            ValidationTypes validationType)
        {
            Message = message;
            ValidationType = validationType;
        }
    }
}
