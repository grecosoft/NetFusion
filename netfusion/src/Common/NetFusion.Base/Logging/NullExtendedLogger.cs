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
        public void Write<TContext>(LogMessage message) { }
        public void Write<TContext>(params LogMessage[] messages) { }
        public void Write<TContext>(IEnumerable<LogMessage> messages) { }
        public void Write<TContext>(LogLevel logLevel, string message, params object[] args) { }
        
        public void WriteDetails<TContext>(LogLevel logLevel, string message, object details, params object[] args) { }
       
        public void Error<TContext>(Exception ex, string message, params object[] args) { }
        public void ErrorDetails<TContext>(Exception ex, string message, object details, params object[] args) { }
        public void Error<TContext>(NetFusionException ex, string message, params object[] args) { }
    }
}