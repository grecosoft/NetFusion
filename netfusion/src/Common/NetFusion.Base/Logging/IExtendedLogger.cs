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
        void Write(LogMessage message);
        void Write(IEnumerable<LogMessage> messages);
        
        void Write(LogLevel logLevel, string message, params object[] args);

        void Error(Exception ex, string message, params object[] args);

        void Error(Exception ex, string message, IDictionary<string, object> details = null, params object[] args);
    }
}