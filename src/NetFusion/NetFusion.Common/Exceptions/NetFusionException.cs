using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NetFusion.Common.Exceptions
{
    /// <summary>
    /// Base exception from which all other plug-in specific exceptions derive.
    /// </summary>
    [Serializable]
    public class NetFusionException : Exception
    {
        private const string NETFUSION_DETAILS_VALUE = "NetFusionExDetails";

        public IDictionary<string, object> Details { get; protected set; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public NetFusionException()
        {
    
        }

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
        /// <param name="innerException">The source of the exception.</param>
        public NetFusionException(string message, Exception innerException)
            : base(message, innerException)
        {

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
