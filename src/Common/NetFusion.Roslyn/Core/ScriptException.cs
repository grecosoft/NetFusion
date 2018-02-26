using NetFusion.Base.Exceptions;
using System;

#if NET461
using System.Runtime.Serialization;
#endif

namespace NetFusion.Domain.Roslyn
{
    /// <summary>
    /// Exception thrown by script evaluation.
    /// </summary>
#if NET461
    [Serializable]
#endif
    public class ScriptException : NetFusionException
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ScriptException()
        {

        }

#if NET461
        public ScriptException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
#endif

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Message describing the script evaluation exception.</param>
        public ScriptException(string message)
            : base(message)
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
        /// <param name="detailKey">Identifies the exception details.</param>
        /// <param name="details">Object containing detailed information about the application
        /// state at the time of the exception.</param>
        public ScriptException(string message, string detailKey, object details)
            : base(message, detailKey, details)
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
