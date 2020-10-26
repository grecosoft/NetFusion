using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Extends Microsoft's ILogger to support structured events.
    /// </summary>
    public static class LoggerExtensions
    {
        public static void Write<TContext>(this ILogger<TContext> logger,
            LogMessage message) => 
            NfExtensions.Logger.Write<TContext>(message);

        public static void Write<TContext>(this ILogger<TContext> logger,
            IEnumerable<LogMessage> messages) => 
            NfExtensions.Logger.Write<TContext>(messages);

        public static void WriteDetails<TContext>(this ILogger<TContext> logger,
            LogLevel logLevel, string message, object details,
            params object[] args) => 
            NfExtensions.Logger.WriteDetails<TContext>(logLevel, message, details, args);

        public static void ErrorDetails<TContext>(this ILogger<TContext> logger,
            Exception ex, string message, object details,
            params object[] args) => 
            NfExtensions.Logger.ErrorDetails<TContext>(ex, message, details, args);

        public static void Error<TContext>(this ILogger<TContext> logger,
            NetFusionException ex, string message,
            params object[] args) => NfExtensions.Logger.Error<TContext>(ex, message, args);
        
        public static DurationLogger LogInformationDuration(this ILogger logger, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), 
                "Logger cannot be null.");

            if (processName == null) throw new ArgumentException(
                "Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogInformation("Start Process: {ProcessName}", processName);

            return new DurationLogger(logger, processName, logger.LogInformation);
        }

        public static DurationLogger LogTraceDuration(this ILogger logger, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), 
                "Logger cannot be null.");

            if (processName == null) throw new ArgumentException(
                "Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogTrace("Start Process: {ProcessName}", processName);

            return new DurationLogger(logger, processName, logger.LogTrace);
        }
    }
}