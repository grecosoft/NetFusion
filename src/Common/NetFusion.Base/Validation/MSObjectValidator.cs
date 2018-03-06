using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetFusion.Base.Validation
{
    /// <summary>
    /// Validation based on Microsoft DataAnnotations.  This is the IObjectValidator implementation
    /// used by default if not overridden when bootstrapping the application container.
    /// </summary>
    public class MSObjectValidator : IObjectValidator
    {
        private List<ValidationItem> _items;
        private readonly List<IObjectValidator> _children;

        public object Object { get; }

        public MSObjectValidator(object obj)
        {            
            Object = obj ?? throw new ArgumentNullException(nameof(obj));

            _items = new List<ValidationItem>();
            _children = new List<IObjectValidator>();
        }

        public IReadOnlyCollection<ValidationItem> Validations => _items;
        public IReadOnlyCollection<IObjectValidator> Children => _children;

        public bool IsValid =>
           !_items.Any(i => i.ValidationType == ValidationTypes.Error) &&
               _children.All(cv => cv.IsValid);

        public ValidationResultSet Validate()
        {
            ValidateObject();
            return new ValidationResultSet(Object, this);
        }

        private void ValidateObject()
        {
            var valResults = new List<ValidationResult>();
            var context = new ValidationContext(Object);

            Validator.TryValidateObject(Object, context, valResults, true);

            var validationItems = valResults.Select(r => new ValidationItem(
                r.ErrorMessage,
                r.MemberNames,
                ValidationTypes.Error)).ToList();

            _items.AddRange(validationItems);

            if (IsValid && Object is IValidatableType validatable)
            {
                validatable.Validate(this);
            }
        }

        public bool Verify(bool predicate, string message, 
            ValidationTypes level = ValidationTypes.Error)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message),
                "Message cannot be null or empty string.");

            if (! predicate)
            {
                _items.Add(new ValidationItem(message, level));
            }
            return predicate;
        }

        public IObjectValidator AddChild(object childObject)
        {
            if (childObject == null) throw new ArgumentNullException(nameof(childObject), 
                "Child object to validate cannot be null.");

            var validator = new MSObjectValidator(childObject);
            validator.ValidateObject();

            _children.Add(validator);
            return validator;
        }
    }
}
