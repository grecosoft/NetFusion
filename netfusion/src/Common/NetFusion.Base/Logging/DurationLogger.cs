using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Logs the time required to execute a block of code.
    /// </summary>
    public class DurationLogger : IDisposable
    {
        private readonly Action<string, object[]> _logMessage;
        private readonly Stopwatch _stopWatch;
        private readonly string _processName;

        public ILogger Log { get; }

        private DurationLogger(ILogger logger,
            Action<string, object[]> logMessage)
        {
            Log = logger ?? throw new ArgumentNullException(nameof(logger));
            _logMessage = logMessage ?? throw new ArgumentNullException(nameof(logger));

            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        /// <summary>
        /// Creates a new duration logger.
        /// </summary>
        /// <param name="logger">The associated logger.</param>
        /// <param name="processName">The name to identify the executed code being logged.</param>
        /// <param name="logMessage">Delegate called to log the message.</param>
        public DurationLogger(ILogger logger,
            string processName,
            Action<string, object[]> logMessage) : this(logger, logMessage)
        {
            if (string.IsNullOrWhiteSpace(processName)) throw new ArgumentException(
                "Process name to log duration of cannot be null", nameof(processName));

            _processName = processName;
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            _logMessage("End Process: {ProcessName}, {ElapsedMs}", new object[] { _processName, _stopWatch.ElapsedMilliseconds });
        }
    }
}
