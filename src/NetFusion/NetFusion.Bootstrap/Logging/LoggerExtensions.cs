using NetFusion.Common;
using NetFusion.Common.Extensions;
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
        /// Logs a verbose message by calling the message delegate if the verbose
        /// level is enabled.  
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message delegate.</param>
        public static void Verbose(this IContainerLogger logger, Func<string> message)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(message, nameof(message));

            if (logger.IsVerboseLevel)
            {
                logger.Verbose(message());
            }
        }

        /// <summary>
        /// Logs a debug message by calling the message delegate if the debug
        /// level is enabled.  
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message delegate.</param>
        public static void Debug(this IContainerLogger logger, Func<string> message)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(message, nameof(message));

            if (logger.IsDebugLevel)
            {
                logger.Debug(message());
            }
        }

        public static DurationLogger ForDuration(this IContainerLogger logger, string processName)
        {
            return new DurationLogger(logger, processName);
        }

        public static void Debug(this IContainerLogger logger, string message, object details)
        {
            logger.Debug(message, details.ToDictionary());
        }
    }

}
