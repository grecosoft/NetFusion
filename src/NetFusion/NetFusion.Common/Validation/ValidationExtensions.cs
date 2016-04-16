using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.Common.Validation
{
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validates the model using ComponentModel validation.
        /// </summary>
        /// <param name="model">The model to validate.</param>
        /// <returns>The result of the validation.</returns>
        public static ValidationResult Validate(this object model)
        {
            Check.NotNull(model, "model");

            var valResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, valResults, true);

            var validationMsgs = new List<ValidationMessage>(valResults.Count);
            foreach (System.ComponentModel.DataAnnotations.ValidationResult result in valResults)
            {
                var entityValidationMsg = result as ValidationMessage;
                if (entityValidationMsg != null)
                {
                    validationMsgs.Add(entityValidationMsg);
                    continue;
                }

                validationMsgs.Add(new ValidationMessage(result, ValidationLevelTypes.Error));
            }
            return new ValidationResult(model, validationMsgs);
        }
    }
}
