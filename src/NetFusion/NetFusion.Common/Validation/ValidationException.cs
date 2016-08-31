using NetFusion.Common.Exceptions;
using System;
using System.Collections.Generic;

namespace NetFusion.Common.Validation
{
    /// <summary>
    /// Exception that is thrown when an entity is determined to be
    /// invalid.
    /// </summary>
    public class ValidationException : NetFusionException
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
        public ObjectValidator Result { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="invalidEntity">The entity that failed validation.</param>
        /// <param name="notifyClient">Indicates if client should be notified.</param>
        /// <param name="result"></param>
        public ValidationException(
            object invalidEntity,
            bool notifyClient,
            ObjectValidator result): base("Validation Exceptions", GetValidationDetails(invalidEntity, result))
        {
            Check.NotNull(invalidEntity, nameof(invalidEntity));
            Check.NotNull(result, nameof(result));

            this.InvalidEntity = invalidEntity;
            this.NotifyClient = notifyClient;
            this.Result = result;
        }

        private static object GetValidationDetails(object invalidEntity, ObjectValidator result)
        {
            return new
            {
                Entity = invalidEntity,
                Result = result
            };
        }
    }
}
