using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Common.Validation
{
    /// <summary>
    /// Returned from validating an entity.  Contains properties and methods for
    /// inspecting the validation result and taking actions.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// The entity that failed validation.
        /// </summary>
        /// <returns>Domain Entity.</returns>
        public object InvalidEntity { get; private set; }

        /// <summary>
        /// The failed validation messages associated with the entity.
        /// </summary>
        public IEnumerable<ValidationMessage> Messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invalidEntity">The entity that failed validation.</param>
        /// <param name="messages">The failed validation messages associated with the entity.</param>
        public ValidationResult(object invalidEntity, IEnumerable<ValidationMessage> messages)
        {
            Check.NotNull(invalidEntity, "invalidEntity");
            Check.NotNull(messages, "messages");

            this.InvalidEntity = invalidEntity;
            this.Messages = messages;
        }

        /// <summary>
        /// The result contains no validation messages.
        /// </summary>
        /// <returns></returns>
        public bool IsValid
        {
            get { return !this.Messages.Any(); }
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
            if (this.Messages.Any(m => m.ValidationLevel >= minMessageType))
            {
                throw new ValidationException(this.InvalidEntity, notifyClient, this.Messages);
            }
        }
    }
}
