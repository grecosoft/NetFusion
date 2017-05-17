using NetFusion.Common;
using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;

#if NET461
using System.Runtime.Serialization;
#endif

namespace NetFusion.Base.Exceptions
{
    /// <summary>
    /// Base exception from which all other plug-in specific exceptions derive.
    /// </summary>
#if NET461
    [Serializable]
#endif
    public class NetFusionException : Exception
    {
        private const string NETFUSION_DETAILS_VALUE = "NetFusionExDetails";

        /// <summary>
        /// Dictionary of key/value pairs containing details of the exception.
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
            this.Details = (IDictionary<string, object>)info.GetValue(NETFUSION_DETAILS_VALUE, typeof(IDictionary<string, object>));
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
        /// <param name="details">Object containing details of the application's state
        /// at the time of the exception.</param>
        public NetFusionException(string message, object details)
            : base(message)
        {
            Check.NotNull(details, nameof(details));

            this.Details = details.ToDictionary();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The message describing the exception.</param>
        /// <param name="details">Object containing details of the application's state
        /// at the time of the exception.</param>
        /// <param name="innerException">The source of the exception.</param>
        public NetFusionException(string message, object details, Exception innerException)
            : base(message, innerException)
        {
            Check.NotNull(details, nameof(details));

            this.Details = details.ToDictionary();
        }
    }
}
