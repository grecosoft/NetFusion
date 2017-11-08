//using NetFusion.Base.Exceptions;
//using NetFusion.Common;
//using NetFusion.Utilities.Validation.Results;

//namespace NetFusion.Utilities.Validation
//{
//    /// <summary>
//    /// Exception that is thrown when an object is determined to be
//    /// invalid.
//    /// </summary>
//    public class ValidationResultException : NetFusionException
//    {
//        /// <summary>
//        /// Reference to the object that failed validation.
//        /// </summary>
//        /// <returns>The object that failed validation.</returns>
//        public object InvalidObject { get; private set; }

//        /// <summary>
//        /// Indicates if the client who made the request should be notified of the
//        /// validation exception.  This should only be set if the validations can
//        /// can be viewed by the client.
//        /// </summary>
//        /// <returns>Boolean value.</returns>
//        public bool NotifyClient { get; private set; }

//        /// <summary>
//        /// The associated validation messages.
//        /// </summary>
//        /// <returns>List of messages.</returns>
//        public ValidationResult Result { get; private set; }

//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="invalidObject">The object that failed validation.</param>
//        /// <param name="notifyClient">Indicates if client should be notified.</param>
//        /// <param name="result"></param>
//        public ValidationResultException(
//            object invalidObject,
//            bool notifyClient,
//            ValidationResult result) : base("Validation Exceptions", GetValidationDetails(invalidObject, result))
//        {
//            Check.NotNull(invalidObject, nameof(invalidObject));
//            Check.NotNull(result, nameof(result));

//            this.InvalidObject = invalidObject;
//            this.NotifyClient = notifyClient;
//            this.Result = result;
//        }

//        private static object GetValidationDetails(object invalidObject, ValidationResult result)
//        {
//            return new
//            {
//                Object = invalidObject,
//                Result = result
//            };
//        }
//    }
//}
