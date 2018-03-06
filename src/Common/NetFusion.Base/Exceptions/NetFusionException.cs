using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;

#if NET461
using System.Runtime.Serialization;
#endif

namespace NetFusion.Base.Exceptions
{
    /// <summary>
    /// Base exception from which all other NetFusion specific exceptions derive.
    /// </summary>
#if NET461
    [Serializable]
#endif
    public class NetFusionException : Exception
    {
        private const string NETFUSION_DETAILS_VALUE = "NetFusionExDetails";

        /// <summary>
        /// Dictionary of key/value pairs containing details of the exception.  This property
        /// can be logged as JSON to provide a detailed description of the exception.
        /// </summary>
        public IDictionary<string, object> Details { get; protected set; } = new Dictionary<string, object>();

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public NetFusionException()
        {
    
        }

        #if NET461
        public NetFusionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Details = (IDictionary<string, object>)info.GetValue(NETFUSION_DETAILS_VALUE, typeof(IDictionary<string, object>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(NETFUSION_DETAILS_VALUE, this.Details);
            base.GetObjectData(info, context);
        }
        #endif

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        public NetFusionException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="innerException">The source of the exception.  If an exception deriving 
        /// from NetFusionException, the details will be added as inner details to this exception.</param>
        public NetFusionException(string message, Exception innerException)
            : base(message, innerException)
        {
            Details["Message"] = message;
            Details["InnerMessage"] = innerException.Message;

            var innerDetails = (innerException as NetFusionException)?.Details;
            if (innerDetails != null)
            {
                Details["InnerDetails"] = innerDetails;
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="detailKey">Value used to identify the exception details.</param>
        /// <param name="details">Object containing details of the application's state
        /// at the time of the exception.</param>
        public NetFusionException(string message, string detailKey, object details)
            : base(message)
        {
            if (String.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
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
        public NetFusionException(string message, Exception innerException, string detailKey, object details)
            : base(message, innerException)
        {
            if (String.IsNullOrWhiteSpace(detailKey)) throw new ArgumentException(
                "Key to identify exception details not specified.", nameof(detailKey));

            Details[detailKey] = details ?? throw new ArgumentNullException(nameof(details),
                "Exception details cannot be null.");
        }
    }
}
