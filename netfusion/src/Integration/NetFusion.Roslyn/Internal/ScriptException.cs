using System;
using NetFusion.Base.Exceptions;

namespace NetFusion.Roslyn.Internal
{
    /// <summary>
    /// Exception thrown by script evaluation.
    /// </summary>
    public class ScriptException : NetFusionException
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ScriptException()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the script evaluation exception.</param>
        public ScriptException(string message) : base(message)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message">Message describing the script evaluation exception.</param>
        /// <param name="innerException">The source exception containing details.</param>
        public ScriptException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the script evaluation exception.</param>
        /// <param name="innerException">The source exception containing details.</param>
        /// <param name="detailKey">Identifies the exception details.</param>
        /// <param name="details">Object containing detailed information about the application
        /// state at the time of the exception.</param>
        public ScriptException(string message, Exception innerException, string detailKey, object details)
            : base(message, innerException, detailKey, details)
        {

        }
    }
}
