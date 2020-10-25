using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Interface implemented to provide extended logging to the base Microsoft ILogger.
    /// </summary>
    public interface IExtendedLogger
    {
        void Write<TContext>(LogMessage message);
        void Write<TContext>(IEnumerable<LogMessage> messages);
        
        void Write<TContext>(LogLevel logLevel, string message, params object[] args);
        void Error<TContext>(Exception ex, string message, params object[] args);
        void Error<TContext>(Exception ex, string message, IDictionary<string, object> details = null, params object[] args);
    }
}