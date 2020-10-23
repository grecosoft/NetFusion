using System;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Interface implemented to provide extended logging
    /// to the base Microsoft ILogger.
    /// </summary>
    public interface IExtendedLogger
    {
        void Add(LogLevel logLevel, string message, params object[] args);

        void Write(LogLevel logLevel, LogMessage message);

        void Error(Exception ex, string message, params object[] args);
    }
}