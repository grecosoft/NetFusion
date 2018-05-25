using System.Collections.Generic;
using NetFusion.Base.Validation;
using System.Linq;

namespace Demo.Infra
{
    public class CustomObjectValidator : IObjectValidator
    {
        public object Object { get; }
        public bool IsValid { get; private set; }

        private List<ValidationItem> _validations = new List<ValidationItem>();
        private List<IObjectValidator> _children = new List<IObjectValidator>();

        public CustomObjectValidator(object obj)
        {
            Object = obj;
        }

        public IEnumerable<ValidationItem> Validations => _validations;
        public IEnumerable<IObjectValidator> Children => _children;

        public IObjectValidator AddChild(object childObject)
        {
            var childValidator = new CustomObjectValidator(childObject);

            _children.Add(childValidator);
            return childValidator;
        }

        public ValidationResultSet Validate()
        {
            var objProperties = Object.GetType().GetProperties();

            IsValid = objProperties.Any(p1 => p1.Name == "FirstName") && 
                objProperties.Any(p2 => p2.Name == "LastName");

            if (IsValid) 
            {
                string firstName = Object.GetType().GetProperty("FirstName").GetValue(Object) as string;
                string lastName = Object.GetType().GetProperty("LastName").GetValue(Object) as string;

                IsValid = firstName == "Mark" && lastName == "Twain";
            }

            if (!IsValid)
            {
                _validations.Add(new ValidationItem(
                    "You are not Mark Twain.", 
                    new [] {"FirstName", "LastName"}, 
                    ValidationTypes.Error));

                return new ValidationResultSet(Object, this);
            }

            return ValidationResultSet.ValidResult(Object);
        }

        public bool Verify(bool predicate, string message, ValidationTypes level = ValidationTypes.Error)
        {
            if (!predicate)
            {
                _validations.Add(new ValidationItem(message, level));
            }
            return predicate;
        }
    }
}