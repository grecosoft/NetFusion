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
        /// <summary>
        /// Writes log message containing a set of detailed properties.
        /// </summary>ı
        /// <param name="logger">Logger instance.</param>
        /// <param name="message">The message to log.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public static void Log<TContext>(this ILogger<TContext> logger,
            LogMessage message) => 
            NfExtensions.Logger.Log<TContext>(message);
        
        /// <summary>
        /// Writes multiple messages containing sets of detailed properties.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="message">Messages to write to the log.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public static void Log<TContext>(this ILogger<TContext> logger,
            params LogMessage[] message) => NfExtensions.Logger.Log<TContext>(message);

        /// <summary>
        /// Writes a list of log messages containing sets of detailed properties.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="messages">Messages to write to the log.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public static void Log<TContext>(this ILogger<TContext> logger,
            IEnumerable<LogMessage> messages) => NfExtensions.Logger.Log<TContext>(messages);

        /// <summary>
        /// Writes log message containing a set of detailed properties associated with
        /// an exception.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <typeparam name="TContext">Namespace associated with the log message.</typeparam>
        public static void Log<TContext>(this ILogger<TContext> logger,
            NetFusionException ex, 
            LogMessage message) => NfExtensions.Logger.Log<TContext>(ex, message);

        /// <summary>
        /// Writes log message containing set of arguments and a detailed child log property.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="logLevel">Associated log level.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="details">Details stored as a log property.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public static void LogDetails<TContext>(this ILogger<TContext> logger,
            LogLevel logLevel, 
            string message, 
            object details, 
            params object[] args) => NfExtensions.Logger.LogDetails<TContext>(logLevel, message, details, args);

        /// <summary>
        /// Writes an error message containing set of arguments for an exception.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="ex">The exception to be logged.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="details">Details stored as a log property.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public static void LogErrorDetails<TContext>(this ILogger<TContext> logger,
            Exception ex, 
            string message, 
            object details,
            params object[] args) => NfExtensions.Logger.LogErrorDetails<TContext>(ex, message, details, args);

        /// <summary>
        /// Writes an error message containing set of arguments for an exception.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="ex">The exception to be logged.</param>
        /// <param name="message">The message template to log.</param>
        /// <param name="args">Optional message template argument values.</param>
        /// <typeparam name="TContext">Namespace associated with log message.</typeparam>
        public static void LogError<TContext>(this ILogger<TContext> logger,
            NetFusionException ex, 
            string message,
            params object[] args) => NfExtensions.Logger.LogError<TContext>(ex, message, args);
        
        /// <summary>
        /// Writes an information level log containing the duration of the specified process.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="processName">The process name associated with the duration.</param>
        /// <returns>Reference to the duration logger.</returns>
        public static DurationLogger LogInformationDuration(this ILogger logger, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), 
                "Logger cannot be null.");

            if (processName == null) throw new ArgumentException(
                "Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogInformation("Start Process: {ProcessName}", processName);

            return new DurationLogger(logger, processName, logger.LogInformation);
        }

        /// <summary>
        /// Writes an debug level log containing the duration of the specified process.
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="processName">The process name associated with the duration.</param>
        /// <returns>Reference to the duration logger.</returns>
        public static DurationLogger LogDebugDuration(this ILogger logger, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), 
                "Logger cannot be null.");

            if (processName == null) throw new ArgumentException(
                "Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogDebug("Start Process: {ProcessName}", processName);

            return new DurationLogger(logger, processName, logger.LogDebug);
        }
    }
}