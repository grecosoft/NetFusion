using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Base.Exceptions
{
    /// <summary>
    /// Base exception from which all other NetFusion specific exceptions derive.
    /// </summary>
    public class NetFusionException : Exception
    {
        public string ExceptionId { get; }
        
        public IEnumerable<Exception> ChildExceptions { get; protected set; }
        
        /// <summary>
        /// Dictionary of key/value pairs containing details of the exception. 
        /// </summary>
        public IDictionary<string, object> Details { get; protected set; } = new Dictionary<string, object>();
        
        protected NetFusionException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        protected NetFusionException(string message, string exceptionId = null)
            : base(message)
        {
            ExceptionId = exceptionId;
            Details["Message"] = message;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="innerException">The source of the exception.  If an exception deriving 
        /// from NetFusionException, the details will be added as inner details to this exception.</param>
        /// <<param name="addToDetails">Determines if a details entry should be added for inner exception.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        protected NetFusionException(string message, Exception innerException, 
            bool addToDetails = true,       
            string exceptionId = null)
            : base(message, innerException)
        {
            ExceptionId = exceptionId;
            Details["Message"] = message;

            AddExceptionDetails(innerException, addToDetails);
        }

        protected NetFusionException(string message, AggregateException aggregateException)
            : base(message, aggregateException.InnerException)
        {
            ChildExceptions = aggregateException.Flatten().InnerExceptions;
            
            Details["Message"] = message;
            Details["ExceptionDetails"] = ChildExceptions.Select(ex => ex.ToString()).ToArray();
        }

        protected void AddExceptionDetails(Exception innerException, bool addToDetails = true)
        {
            if (addToDetails)
            {
                Details["InnerException"] = innerException.ToString();
            }
            
            if (innerException is NetFusionException detailedEx && detailedEx.Details != null)
            {
                Details["InnerDetails"] = detailedEx.Details;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="detailKey">Value used to identify the exception details.</param>
        /// <param name="details">Object containing details of the application's state
        /// at the time of the exception.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        protected NetFusionException(string message, string detailKey, object details, 
            string exceptionId = null)
            : this(message, exceptionId)
        {
            if (string.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
                "Key to identify exception details not specified.", nameof(detailKey));

            Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
                "Exception details cannot be null.");
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="innerException">The source of the exception.</param>
        /// <param name="detailKey">Value used to identify the exception details.</param>
        /// <param name="details">Object containing details of the application's state
        /// at the time of the exception.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        protected NetFusionException(string message, Exception innerException, 
            string detailKey, 
            object details, 
            string exceptionId = null) : this(message, innerException, exceptionId: exceptionId)
        {
            if (string.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
                "Key to identify exception details not specified.", nameof(detailKey));

            Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
                "Exception details cannot be null.");
        }
    }
}
