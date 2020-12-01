using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Default implementation specified as the extended logger that does
    /// not attempt to log the message.  This is a place holder for the
    /// real implementation used by the host.  See: NetFusion.Serilog.
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Log<TContext>(LogMessage message) { }
        public void Log<TContext>(params LogMessage[] messages) { }
        public void Log<TContext>(IEnumerable<LogMessage> messages) { }
        public void Log<TContext>(NetFusionException ex, LogMessage message) { }

        public void Log<TContext>(LogLevel logLevel, string message, params object[] args) { }
        
        public void LogDetails<TContext>(LogLevel logLevel, string message, object details, params object[] args) { }
       
        public void LogError<TContext>(Exception ex, string message, params object[] args) { }
        public void LogErrorDetails<TContext>(Exception ex, string message, object details, params object[] args) { }
        public void LogError<TContext>(NetFusionException ex, string message, params object[] args) { }
    }
}