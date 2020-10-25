using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    public static class LoggerExtensions
    {
        public static void Write<TContext>(this ILogger<TContext> logger,  LogMessage message)
        {
            NfExtensions.Logger.Write<TContext>(message);
        }

        public static void Write<TContext>(this ILogger<TContext> logger, IEnumerable<LogMessage> messages)
        {
            NfExtensions.Logger.Write<TContext>(messages);
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
    }
}