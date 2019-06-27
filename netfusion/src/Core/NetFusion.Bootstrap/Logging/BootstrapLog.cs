using System;
using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Represents a log entry written by the bootstrap process
    /// before the ILogger is available. 
    /// </summary>
    public class BootstrapLog
    {
        public LogLevel LogLevel { get; }
        public string Message { get; }

        public object[] Args { get; set; }

        public BootstrapLog(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}