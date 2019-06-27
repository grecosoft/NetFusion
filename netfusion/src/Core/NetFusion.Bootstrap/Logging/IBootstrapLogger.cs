using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Logger used before the .net core ILogger instance is available.
    /// </summary>
    public interface IBootstrapLogger
    {
        /// <summary>
        /// All the current recorded logs.
        /// </summary>
        IEnumerable<BootstrapLog> Logs { get; }
        
        /// <summary>
        /// Determines if any of the log entries is an error.
        /// </summary>
        bool HasErrors { get; }
        
        void Add(LogLevel logLevel, string message, params object[] args);
        
        /// <summary>
        /// Writes the recorded logs to standard-out.
        /// </summary>
        void WriteToStandardOut();
        
        /// <summary>
        /// Writes the recorded logs to the provided logger.
        /// </summary>
        /// <param name="logger">The logger available after the service-provider
        /// has been created.</param>
        void WriteToLogger(ILogger logger);
    }
}