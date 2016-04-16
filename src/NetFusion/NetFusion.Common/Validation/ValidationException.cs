using System;
using System.Collections.Generic;

namespace NetFusion.Common.Validation
{
    /// <summary>
    /// Exception that is thrown when an entity is determined to be
    /// invalid.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Reference to the entity that failed validation.
        /// </summary>
        /// <returns>Domain entity.</returns>
        public object InvalidEntity { get; private set; }

        /// <summary>
        /// Indicates if the client who made the request should be notified of the
        /// validation exception.  This should only be set if the validation entity
        /// and messages that can be viewed by the client.
        /// </summary>
        /// <returns>Boolean value.</returns>
        public bool NotifyClient { get; private set; }

        /// <summary>
        /// The associated validation messages.
        /// </summary>
        /// <returns>List of messages.</returns>
        public IEnumerable<ValidationMessage> ValidationMessages { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="invalidEntity">The entity that failed validation.</param>
        /// <param name="notifyClient">Indicates if client should be notified.</param>
        /// <param name="validationMessages">Validation messages.</param>
        public ValidationException(
            object invalidEntity,
            bool notifyClient,
            IEnumerable<ValidationMessage> validationMessages)
        {
            Check.NotNull(invalidEntity, "invalidEntity");
            Check.NotNull(validationMessages, "validationMessages");

            this.InvalidEntity = invalidEntity;
            this.NotifyClient = notifyClient;
            this.ValidationMessages = validationMessages;
        }
    }
}
