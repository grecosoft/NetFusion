using System;
using System.Collections.Generic;

namespace NetFusion.Base.Exceptions
{
    /// <summary>
    /// Base exception from which all other NetFusion specific exceptions derive.
    /// </summary>
    public class NetFusionException : Exception
    {
        /// <summary>
        /// Dictionary of key/value pairs containing details of the exception. 
        /// </summary>
        public IDictionary<string, object> Details { get; protected set; } = new Dictionary<string, object>();
        
        protected NetFusionException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        protected NetFusionException(string message)
            : base(message)
        {
            Details["Message"] = message;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="innerException">The source of the exception.  If an exception deriving 
        /// from NetFusionException, the details will be added as inner details to this exception.</param>
        /// <<param name="recordException">Determines if a a detail entry should be added for the exception.</param>
        protected NetFusionException(string message, Exception innerException, bool addToDetails = true)
            : base(message, innerException)
        {
            Details["Message"] = message;

            AddExceptionDetails(innerException, addToDetails);
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
        protected NetFusionException(string message, string detailKey, object details)
            : this(message)
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
        protected NetFusionException(string message, Exception innerException, 
            string detailKey, 
            object details) : this(message, innerException)
        {
            if (string.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
                "Key to identify exception details not specified.", nameof(detailKey));

            Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
                "Exception details cannot be null.");
        }
    }
}
