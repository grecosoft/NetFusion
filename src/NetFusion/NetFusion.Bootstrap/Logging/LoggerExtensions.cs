using NetFusion.Common;
using System;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Additional logging methods associated with the logger that don't
    /// have to be implemented by the IContainerLogger instance.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs a verbose message by calling the details delegate if the verbose
        /// level is enabled.  This should be used if there is time associated
        /// with building the details to be logged.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message delegate.</param>
        /// <param name="details">Details associated with the log message.</param>
        public static void Verbose(this IContainerLogger logger, string message, Func<object> details)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(message, nameof(message));
            Check.NotNull(details, nameof(details));

            if (logger.IsVerboseLevel)
            {
                logger.Verbose(message, details());
            }
        }

        /// <summary>
        /// Logs a debug message by calling the details delegate if the debug
        /// level is enabled.  This should be used if there is time associated
        /// with building the details to be logged.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message delegate.</param>
        /// /// <param name="details">Details associated with the log message.</param>
        public static void Debug(this IContainerLogger logger, string message, Func<object> details)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(message, nameof(message));
            Check.NotNull(details, nameof(details));

            if (logger.IsDebugLevel)
            {
                logger.Debug(message, details());
            }
        }

        /// <summary>
        /// Returns a logger that can be used to write a debug log for the time required
        /// to execute a block of code.
        /// </summary>
        /// <param name="logger">The associated logger.</param>
        /// <param name="processName">Name used to described the block of code being executed.</param>
        /// <returns>Logger used to log a duration.</returns>
        public static DurationLogger DebugDuration(this IContainerLogger logger, string processName)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNullOrWhiteSpace(processName, nameof(processName));

            return new DurationLogger(logger, processName, logger.Debug);
        }

        /// <summary>
        /// Returns a logger that can be used to write a verbose log for the time required
        /// to execute a block of code.
        /// </summary>
        /// <param name="logger">The associated logger.</param>
        /// <param name="processName">Name used to described the block of code being executed.</param>
        /// <returns>Logger used to log a duration.</returns>
        public static DurationLogger VerboseDuration(this IContainerLogger logger, string processName, object details = null)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNullOrWhiteSpace(processName, nameof(processName));

            return new DurationLogger(logger, processName, logger.Verbose, details);
        }
    }

}
