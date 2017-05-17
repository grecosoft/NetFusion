using Microsoft.Extensions.Logging;
using System;

namespace NetFusion.Testing.Logging
{
    /// <summary>
    /// Used to store the state of log messages written to the test-logger.
    /// </summary>
    public class LogEntry
    {
        public int EventId { get; set; }
        public LogLevel Level { get; set; }
        public Exception Exception { get; set; }
    }
}
