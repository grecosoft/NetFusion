using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Logger used before the .net core ILogger instance is available.
    /// </summary>
    public interface IBootstrapLogger
    {
        BootstrapLog[] Logs { get; }
        void Add(LogLevel logLevel, string message, params object[] args);
        void WriteToStandardOut();
    }
}