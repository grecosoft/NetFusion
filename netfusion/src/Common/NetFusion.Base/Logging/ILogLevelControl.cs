using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Interface implemented by a component responsible
    /// for changing the log level at runtime.
    /// </summary>
    public interface ILogLevelControl
    {
        /// <summary>
        /// Sets the minimum log level at runtime.
        /// </summary>
        /// <param name="logLevel">Log level.</param>
        /// <returns>The set log level.</returns>
        string SetMinimumLevel(LogLevel logLevel);
    }
}