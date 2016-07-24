using System.Collections.Generic;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Interface that can be implemented by the host application
    /// to log to a specific logger implementation.
    /// </summary>
    public interface IContainerLogger
    {
        /// <summary>
        /// Indicates if the verbose level is enabled.
        /// </summary>
        bool IsVerboseLevel { get; }

        /// <summary>
        /// Indicates the information level is enabled.
        /// </summary>
        bool IsInfoLevel { get; }

        /// <summary>
        /// Indicates if the debug level is enabled.
        /// </summary>
        bool IsDebugLevel { get; }

        /// <summary>
        /// Indicates if the warning level is enabled.
        /// </summary>
        bool IsWarningLevel { get; }

        /// <summary>
        /// Indicates if the error level is enabled.
        /// </summary>
        bool IsErrorLevel { get; }

        /// <summary>
        /// Associates a context with the log message.
        /// </summary>
        /// <typeparam name="TContext">The context type.</typeparam>
        /// <returns>New instance of the logger with the configured context.</returns>
        IContainerLogger ForContext<TContext>();

        /// <summary>
        /// Writes a verbose message to the log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">Anonymous object containing details.</param>
        void Verbose(string message, object details = null);

        /// <summary>
        /// Writes an information message to the log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">Anonymous object containing details.</param>
        void Info(string message, object details = null);

        /// <summary>
        /// Writes a debug message to the log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">Anonymous object containing details.</param>
        void Debug(string message, object details = null);

        /// <summary>
        /// Writes a error message to the log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">Anonymous object containing details.</param>
        void Error(string message, object details = null);

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="details">Anonymous object containing details.</param>
        void Warning(string message, object details = null);
    }
}
