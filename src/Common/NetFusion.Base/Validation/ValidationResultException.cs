using NetFusion.Base.Exceptions;
using System;

namespace NetFusion.Base.Validation
{
    /// <summary>
    /// Exception that is thrown when an object is determined to be invalid.
    /// </summary>
    public class ValidationResultException : NetFusionException
    {
        /// <summary>
        /// Reference to the object that failed validation.
        /// </summary>
        /// <returns>The object that failed validation.</returns>
        public object InvalidObject { get; private set; }

        /// <summary>
        /// Indicates if the client who made the request should be notified of the
        /// validation exception.  This should only be set if the validations can
        /// can be viewed by the client.
        /// </summary>
        /// <returns>Boolean value.</returns>
        public bool NotifyClient { get; private set; }

        /// <summary>
        /// The associated validation result set.
        /// </summary>
        /// <returns>The set of invalidations associated with the exception.</returns>
        public ValidationResultSet Result { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="invalidObject">The object that failed validation.</param>
        /// <param name="notifyClient">Indicates if client should be notified.</param>
        /// <param name="result">The set of invalidations associated with the exception.</param>
        public ValidationResultException(
            object invalidObject,
            bool notifyClient,
            ValidationResultSet result) : base(
                "Validation Exceptions", 
                "Validations", 
                GetValidationDetails(invalidObject, result))
        {
            InvalidObject = invalidObject ?? throw new ArgumentNullException(nameof(invalidObject));
            Result = result ?? throw new ArgumentNullException(nameof(result));

            NotifyClient = notifyClient;
        }

        private static object GetValidationDetails(object invalidObject, ValidationResultSet result)
        {
            return new
            {
                Object = invalidObject,
                Result = result
            };
        }
    }
}
