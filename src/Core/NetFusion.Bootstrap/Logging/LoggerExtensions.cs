using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Additional logging methods extending the ILogger interface that allow passing an object
    /// containing additional details to be logged as JSON.
    /// </summary>
    public static class LoggerExtensions
    {
        private static readonly Func<object, Exception, string> _messageFormatter = (obj, ex) => obj.ToString();

        //------------------------------------------DEBUG------------------------------------------//

        public static void LogDebugDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details, object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Debug, eventId, exception, message, args, details);
        }

        public static void LogDebugDetails(this ILogger logger, EventId eventId,
            string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Debug, eventId, null, message, args, details);
        }

        public static void LogDebugDetails(this ILogger logger, string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Debug, 0, null, message, args, details);
        }

        //------------------------------------------TRACE------------------------------------------//

        public static void LogTraceDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details, object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Trace, eventId, exception, message, args, details);
        }

        public static void LogTraceDetails(this ILogger logger, EventId eventId,
            string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Trace, eventId, null, message, args, details);
        }

        public static void LogTraceDetails(this ILogger logger, string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Trace, 0, null, message, args, details);
        }

        public static DurationLogger LogTraceDuration(this ILogger logger, EventId eventId, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (processName == null)
                throw new ArgumentException("Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogTrace(eventId, 
                "Start Process: {ProcessName}", 
                new object[] { processName });

            return new DurationLogger(logger, processName, logger.LogTrace);
        }

        //------------------------------------------INFORMATION------------------------------------------//

        public static void LogInformationDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details, object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Information, eventId, exception, message, args, details);
        }

        public static void LogInformationDetails(this ILogger logger, EventId eventId,
            string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Information, eventId, null, message, args, details);
        }

        public static void LogInformationDetails(this ILogger logger, string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Information, 0, null, message, args, details);
        }

        //------------------------------------------WARNING------------------------------------------//

        public static void LogWarningDetails(this ILogger logger, EventId eventId, Exception exception,
             string message, object details, object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Warning, eventId, exception, message, args, details);
        }

        public static void LogWarningDetails(this ILogger logger, EventId eventId,
             string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Warning, eventId, null, message, args, details);
        }

        public static void LogWarningDetails(this ILogger logger, string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Warning, 0, null, message, args, details);
        }

        //------------------------------------------ERROR------------------------------------------//

        public static void LogErrorDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details, object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Error, eventId, exception, message, args, details);
        }

        public static void LogErrorDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Error, eventId, exception, message, new string[]{}, details);
        }

        public static void LogErrorDetails(this ILogger logger, EventId eventId,
            string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Error, eventId, null, message, args, details);
        }

        public static void LogErrorDetails(this ILogger logger, string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Error, 0, null, message, args, details);
        }

        //------------------------------------------CRITICAL------------------------------------------//

        public static void LogCriticalDetails(this ILogger logger, EventId eventId, Exception exception,
            string message, object details, object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Critical, eventId, exception, message, args, details);
        }

        public static void LogCriticalDetails(this ILogger logger, EventId eventId,
            string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Critical, eventId, null, message, args, details);
        }

        public static void LogCriticalDetails(this ILogger logger, string message, object details, params object[] args)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

            logger.LogDetails(LogLevel.Critical, 0, null, message, args, details);
        }

        private static void LogDetails(this ILogger logger, LogLevel logLevel, EventId eventId,
            Exception exception,
            string message,
            object[] args,
            object details)
        {

            var values = new List<object>(args)
            {
                details.ToIndentedJson()
            };

            logger.Log(logLevel, eventId, new FormattedLogValues(message + " {details}", values.ToArray()), exception, _messageFormatter);
        }
    }
}
