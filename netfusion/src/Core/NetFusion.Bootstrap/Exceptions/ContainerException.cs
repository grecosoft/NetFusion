using NetFusion.Base.Exceptions;
using System;

namespace NetFusion.Bootstrap.Exceptions
{
    /// <summary>
    /// Exception thrown by the container when there is an issue bootstrapping the application.
    /// </summary>
    public class ContainerException : NetFusionException
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ContainerException()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        public ContainerException(string message, string exceptionId = null) 
            : base(message, exceptionId)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        /// <param name="innerException">The source exception containing details.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        public ContainerException(string message, Exception innerException, string exceptionId = null)
            : base(message, innerException, exceptionId: exceptionId)
        {

        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the container bootstrap exception.</param>
        /// <param name="detailKey">Value used to identify the details.</param>
        /// <param name="details">Object containing detailed information about the application
        /// state at the time of the exception.</param>
        /// <param name="exceptionId">Optional value used to identity the exception.</param>
        public ContainerException(string message, string detailKey, object details, string exceptionId = null)
            : base(message, detailKey, details, exceptionId)
        {

        }
    }
}
