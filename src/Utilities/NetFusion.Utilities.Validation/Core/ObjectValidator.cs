using NetFusion.Common;
using NetFusion.Utilities.Validation.Results;
using System.Collections.Generic;
using System.Linq;

using MSValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using MSValidationDataResult = System.ComponentModel.DataAnnotations.ValidationResult;
using MSValidator = System.ComponentModel.DataAnnotations.Validator;

namespace NetFusion.Utilities.Validation.Core
{
    /// <summary>
    /// Validation based on Microsoft DataAnnotations.
    /// </summary>
    public class ObjectValidator : IObjectValidator
    {
        private List<ValidationItem> _items;
        private readonly List<IObjectValidator> _children;

        public object Object { get; }
        public IEnumerable<ValidationItem> Validations => _items;
        public IEnumerable<IObjectValidator> Children => _children;

        public ObjectValidator(object obj)
        {
            Check.NotNull(obj, nameof(obj));

            this.Object = obj;

            _items = ValidateObject(this.Object);
            _children = new List<IObjectValidator>();
        }

        private List<ValidationItem> ValidateObject(object obj)
        {
            var valResults = new List<MSValidationDataResult>();
            var context = new MSValidationContext(obj);

            MSValidator.TryValidateObject(obj, context, valResults, true);

            return valResults.Select(r => new ValidationItem(
                r.ErrorMessage,
                r.MemberNames,
                ValidationTypes.Error)).ToList();
        }

        public bool IsValid =>
            !_items.Any(i => i.ValidationType == ValidationTypes.Error) &&
                _children.All(cv => cv.IsValid);

        public bool Validate(bool predicate, string message, 
            ValidationTypes level = ValidationTypes.Error)
        {
            Check.NotNullOrWhiteSpace(message, nameof(message));

            if (!predicate)
            {
                _items.Add(new ValidationItem(message, level));
            }
            return predicate;
        }

        public IObjectValidator AddChildValidator(object childObject)
        {
            Check.NotNull(childObject, nameof(childObject));

            var validator = new ObjectValidator(childObject);

            var validatable = childObject as IValidatableType;
            if (validator.IsValid && validatable != null)
            {
                validatable.Validate(validator);
            }

            _children.Add(validator);
            return validator;
        }
    }
}
