using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions;
using System;
using NetFusion.Base.Logging;

namespace NetFusion.Bootstrap.Logging
{
    
    // TODO:  Move NetFusion.Base
    
    /// <summary>
    /// Additional logging methods extending the ILogger interface that allow passing an object
    /// containing additional details to be logged as JSON.
    /// </summary>
    public static class LoggerExtensions
    {
        public static void LogTest(this ILogger logger, string message)
        {
            NfExtensions.Logger.Write(LogLevel.Information, message);
        }
        
        
        
        
        
        public static void LogTraceDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            logger.LogDetails(LogLevel.Trace, null, message, details);
        }
        
        public static void LogErrorDetails(this ILogger logger, Exception exception,
            string message)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (exception == null) throw new ArgumentNullException(nameof(exception), "Exception cannot be null.");

            object details = null;
            if (exception is NetFusionException netFusionEx)
            {
                details = netFusionEx.Details;
            }

            logger.LogDetails(LogLevel.Error, exception, message, details);
        }
        
        
        
        
        public static DurationLogger LogInformationDuration(this ILogger logger, string processName)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), 
                "Logger cannot be null.");

            if (processName == null) throw new ArgumentException(
                "Name identifying process being logged cannot be null.", nameof(processName));

            logger.LogTrace("Start Process: {ProcessName}", processName);

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

       

        
        
        
        
        
        
        
        
        private static void LogDetails(this ILogger logger, LogLevel logLevel,
           Exception exception,
           string message,
           object details)
        {
            if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Log message not specified.", nameof(message));
            if (details == null) throw new ArgumentNullException(nameof(details), "Log details cannot be null.");

            string msgDetails = details.ToIndentedJson();

            logger.Log(logLevel, exception,
                message + $" Details: {msgDetails}");
        }
    }
}
