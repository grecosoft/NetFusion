using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions;
using System;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Additional logging methods extending the ILogger interface that allow passing an object
    /// containing additional details to be logged as JSON.
    /// </summary>
    public static class LoggerExtensions
    {
        //------------------------------------------DEBUG------------------------------------------//

        public static void LogDebugDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            logger.LogDetails(LogLevel.Debug, eventId, exception, message, details);
        }

        public static void LogDebugDetails(this ILogger logger, EventId eventId,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Debug, eventId, null, message, details);
        }

        public static void LogDebugDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Debug, 0, null, message, details);
        }

        //------------------------------------------TRACE------------------------------------------//

        public static void LogTraceDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            logger.LogDetails(LogLevel.Trace, eventId, exception, message, details);
        }

        public static void LogTraceDetails(this ILogger logger, EventId eventId,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Trace, eventId, null, message, details);
        }

        public static void LogTraceDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Trace, 0, null, message, details);
        }

        public static DurationLogger LogTraceDuration(this ILogger logger, EventId eventId, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), 
                "Logger cannot be null.");

            if (processName == null) throw new ArgumentException(
                "Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogTrace(eventId, 
                "Start Process: {ProcessName}", processName);

            return new DurationLogger(logger, processName, logger.LogTrace);
        }

        //------------------------------------------INFORMATION------------------------------------------//

        public static void LogInformationDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            logger.LogDetails(LogLevel.Information, eventId, exception, message, details);
        }

        public static void LogInformationDetails(this ILogger logger, EventId eventId,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Information, eventId, null, message, details);
        }

        public static void LogInformationDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Information, 0, null, message, details);
        }

        //------------------------------------------WARNING------------------------------------------//

        public static void LogWarningDetails(this ILogger logger, EventId eventId, Exception exception,
             string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            logger.LogDetails(LogLevel.Warning, eventId, exception, message, details);
        }

        public static void LogWarningDetails(this ILogger logger, EventId eventId,
             string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Warning, eventId, null, message, details);
        }

        public static void LogWarningDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Warning, 0, null, message, details);
        }

        //------------------------------------------ERROR------------------------------------------//

        public static void LogErrorDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            logger.LogDetails(LogLevel.Error, eventId, exception, message, details);
        }

        public static void LogErrorDetails(this ILogger logger, EventId eventId, Exception exception,
            string message)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            object details = null;
            if (exception is NetFusionException netFusionEx)
            {
                details = netFusionEx.Details;
            }

            logger.LogDetails(LogLevel.Error, eventId, exception, message, details);
        }

        public static void LogErrorDetails(this ILogger logger, EventId eventId,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Error, eventId, null, message, details);
        }

        public static void LogErrorDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Error, 0, null, message, details);
        }

        //------------------------------------------CRITICAL------------------------------------------//

        public static void LogCriticalDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            logger.LogDetails(LogLevel.Critical, eventId, exception, message, details);
        }

        public static void LogCriticalDetails(this ILogger logger, EventId eventId,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Critical, eventId, null, message, details);
        }

        public static void LogCriticalDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Critical, 0, null, message, details);
        }

        private static void LogDetails(this ILogger logger, LogLevel logLevel, EventId eventId,
           Exception exception,
           string message,
           object details)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Log message not specified.", nameof(message));
            if (details == null) throw new ArgumentNullException(nameof(details), "Log details cannot be null.");

            string msgDetails = details.ToIndentedJson();

            logger.Log(logLevel, eventId,
                message + $" Details: {msgDetails}",
                exception,
                FormatMessage);
        }

        private static string FormatMessage(object state, Exception ex)
        {
            return state.ToString();
        }
    }
}
