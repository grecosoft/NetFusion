using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions;
using System;

namespace NetFusion.Bootstrap.Logging
{
    
    // TODO:  Move NetFusion.Base
    
    /// <summary>
    /// Additional logging methods extending the ILogger interface that allow passing an object
    /// containing additional details to be logged as JSON.
    /// </summary>
    public static class LoggerExtensions
    {
        //8
        public static void LogTraceDetails(this ILogger logger, string message, object details)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");
            if (details == null) throw new ArgumentNullException(nameof(details));
            
            string msgDetails = details.ToIndentedJson();
            logger.Log(LogLevel.Trace, null, message + $" Details: {msgDetails}");
        }
    }
}
