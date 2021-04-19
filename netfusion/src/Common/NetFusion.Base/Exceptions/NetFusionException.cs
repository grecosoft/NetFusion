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
        /// <summary>
        /// Value use to identity the context of the exception.  Used when asserting
        /// exceptions within unit-tests.
        /// </summary>
        public string ExceptionId { get; }
        
        /// <summary>
        /// List of child exceptions associated with parent exception.  This can be a
        /// list of custom specified exceptions or a flattened listed of exceptions
        /// associated with an asynchronous task.
        /// </summary>
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
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        protected NetFusionException(string message, Exception innerException, string exceptionId = null)
            : base(message, innerException)
        {
            ExceptionId = exceptionId;
            Details["Message"] = message;

            AddExceptionDetails(innerException);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="aggregateException">An aggregate exception associated with a task.</param>
        protected NetFusionException(string message, AggregateException aggregateException)
            : base(message, aggregateException.InnerException)
        {
            ChildExceptions = aggregateException.Flatten().InnerExceptions;
            
            Details["Message"] = message;
            AddExceptionDetails(ChildExceptions);
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
            string exceptionId = null) : this(message, innerException, exceptionId)
        {
            if (string.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
                "Key to identify exception details not specified.", nameof(detailKey));

            Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
                "Exception details cannot be null.");
        }

        protected void AddExceptionDetails(Exception innerException)
        {
            Details["InnerException"] = innerException.ToString();
            
            if (innerException is NetFusionException detailedEx && detailedEx.Details != null)
            {
                Details["InnerDetails"] = detailedEx.Details;
            }
        }

        protected void AddExceptionDetails(IEnumerable<Exception> exceptions)
        {
            var detailedExceptions = exceptions.OfType<NetFusionException>()
                .Select(de => de.Details)
                .Where(d => d != null)
                .ToArray();

            Details["InnerDetails"] = detailedExceptions;
        }
    }
}
