using NetFusion.Base.Exceptions;
using System;

#if NET461
using System.Runtime.Serialization;
#endif

namespace NetFusion.Bootstrap.Exceptions
{
    /// <summary>
    /// Exception thrown by the container when there is an issue bootstrapping the application.
    /// </summary>
#if NET461
    [Serializable]
#endif
    public class ContainerException : NetFusionException
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ContainerException()
        {

        }

        #if NET461
        public ContainerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
        #endif

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        public ContainerException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        /// <param name="innerException">The source exception containing details.</param>
        public ContainerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        /// <param name="details">Object containing detailed information about the application
        /// state at the time of the exception.</param>
        public ContainerException(string message, object details)
            : base(message, details)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        /// <param name="details">Object containing detailed information about the application
        /// state at the time of the exception.</param>
        /// <param name="innerException">The source exception containing details.</param>
        public ContainerException(string message, object details, Exception innerException)
            : base(message, details, innerException)
        {

        }
    }
}
