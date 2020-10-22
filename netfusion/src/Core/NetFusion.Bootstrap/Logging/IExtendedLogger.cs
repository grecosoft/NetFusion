using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Interface implemented to provide extended logging
    /// to the base Microsoft ILogger.
    /// </summary>
    public interface IExtendedLogger
    {
        void Add(LogLevel logLevel, string message, params object[] args);
    }
}