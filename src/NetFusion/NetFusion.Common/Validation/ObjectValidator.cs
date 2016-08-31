using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetFusion.Common.Validation
{
    /// <summary>
    /// Returned from validating an entity.  Contains properties and methods for
    /// inspecting the validation result and taking actions.
    /// </summary>
    public class ObjectValidator
    {
        private readonly object _invalidEntity;

        public Type EntityType { get; private set; }

        /// <summary>
        /// The failed validation messages associated with the entity.
        /// </summary>
        private readonly List<ValidationMessage> _messages;

        private readonly List<ObjectValidator> _childValidations;

        public IEnumerable<ValidationMessage> Messages => _messages;
        public IEnumerable<ObjectValidator> ChildResults => _childValidations;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="entity">The entity that failed validation.</param>
        /// <param name="messages">The failed validation messages associated with the entity.</param>
        public ObjectValidator(object entity, IEnumerable<ValidationMessage> messages)
        {
            Check.NotNull(entity, "invalidEntity");
            Check.NotNull(messages, "messages");

            _invalidEntity = entity;
            this.EntityType = entity.GetType();
            _messages = ValidateObject(entity);
            _childValidations = new List<ObjectValidator>();
        }

        public ObjectValidator(object entity, string message, ValidationLevelTypes level):
            this(entity, new[] { new ValidationMessage(message, level)})
        {

        }

        public ObjectValidator(object entity)
        {
            _invalidEntity = entity;
            this.EntityType = entity.GetType();
            _messages = ValidateObject(entity);
            _childValidations = new List<ObjectValidator>();
        }

        private List<ValidationMessage> ValidateObject(object entity)
        {
            var valResults = new List<ValidationResult>();
            var context = new ValidationContext(entity);
            Validator.TryValidateObject(entity, context, valResults, true);

            var validationMsgs = new List<ValidationMessage>(valResults.Count);
            foreach (ValidationResult result in valResults)
            {
                var entityValidationMsg = result as ValidationMessage;
                if (entityValidationMsg != null)
                {
                    validationMsgs.Add(entityValidationMsg);
                    continue;
                }

                validationMsgs.Add(new ValidationMessage(result, ValidationLevelTypes.Error));
            }

            return validationMsgs;
        }

        public void AddChildValidation(ObjectValidator validation)
        {
            _childValidations.Add(validation);
        }

        public void AddChildValidations(IEnumerable<ObjectValidator> validations)
        {
            _childValidations.AddRange(validations.ToList());
        }

        public bool Guard(bool predicate, string message, ValidationLevelTypes level)
        {
            if (!predicate)
            {
                _messages.Add(new ValidationMessage(message, level));
            }
            return predicate;
        }

        /// <summary>
        /// The result contains no validation messages.
        /// </summary>
        /// <returns></returns>
        public bool IsValid
        {
            get { return this.Messages.Empty() && HasValidChildren();  }
        }

        private bool HasValidChildren()
        {
            return _childValidations.All(v => v.IsValid);
        }

        /// <summary>
        /// Indicates that the there are Info level messages.
        /// </summary>
        /// <returns></returns>
        public bool HasInfo
        {
            get { return this.Messages.Any(m => m.ValidationLevel == ValidationLevelTypes.Info); }
        }

        /// <summary>
        /// Indicates that there are Warning level messages.
        /// </summary>
        /// <returns></returns>
        public bool HasWarnings
        {
            get { return this.Messages.Any(m => m.ValidationLevel == ValidationLevelTypes.Warning); }
        }

        /// <summary>
        /// Indicates that there are Error level messages.
        /// </summary>
        /// <returns></returns>
        public bool HasErrors
        {
            get { return this.Messages.Any(m => m.ValidationLevel == ValidationLevelTypes.Error); }
        }

        /// <summary>
        /// Throws an exception with the entity validation messages if there are messages 
        /// above or equal to a specified level.
        /// </summary>
        /// <param name="minMessageType">The minimal level consider an exception.  
        /// The default is Warning.</param>
        /// <param name="notifyClient">Indicates if the client should be notified of the exception.</param>
        public void ThrowIfNotValid(ValidationLevelTypes minMessageType = ValidationLevelTypes.Warning,
            bool notifyClient = false)
        {
            if (!this.IsValid)
            {
                throw new ValidationException(_invalidEntity, notifyClient, this);
            }
        }
    }
}
