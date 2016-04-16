using System.Collections.Generic;

namespace NetFusion.Common.Validation
{
    /// <summary>
    /// Message containing information about a failed validation.
    /// </summary>
    public class ValidationMessage : System.ComponentModel.DataAnnotations.ValidationResult
    {
        /// <summary>
        /// The level of the validation.
        /// </summary>
        /// <returns>Validation Level</returns>
        public ValidationLevelTypes ValidationLevel { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorMessage">Validation Error message.</param>
        /// <param name="validationLevel">Validation level.</param>
        public ValidationMessage(string errorMessage, ValidationLevelTypes validationLevel)
            : base(errorMessage)
        {
            Check.NotNullOrWhiteSpace(errorMessage, "errorMessage");

            this.ValidationLevel = validationLevel;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="errorMessage">Validation Error message.</param>
        /// <param name="memberNames">The name of the entity's properties pertaining to the error.</param>
        /// <param name="validationLevel">Validation level.</param>
        public ValidationMessage(string errorMessage, IEnumerable<string> memberNames, ValidationLevelTypes validationLevel)
            : base(errorMessage, memberNames)
        {
            Check.NotNullOrWhiteSpace(errorMessage, "errorMessage");
            Check.NotNull(memberNames, "memberNames");

            this.ValidationLevel = validationLevel;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="validationResult">Base validation result object.</param>
        public ValidationMessage(System.ComponentModel.DataAnnotations.ValidationResult validationResult)
            : base(validationResult)
        {
            Check.NotNull(validationResult, "validationResult");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="validationResult">Base validation result object.</param>
        /// <param name="validationLevel">The level associated with the validation.</param>
        public ValidationMessage(System.ComponentModel.DataAnnotations.ValidationResult validationResult, ValidationLevelTypes validationLevel)
            : base(validationResult)
        {
            Check.NotNull(validationResult, "validationResult");
            this.ValidationLevel = validationLevel;
        }
    }
}
