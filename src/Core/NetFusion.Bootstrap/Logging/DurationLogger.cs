using Microsoft.Extensions.Logging;
using NetFusion.Common;
using System;
using System.Diagnostics;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Logs the time required to execute a block of code.
    /// </summary>
    public class DurationLogger : IDisposable
    {
        private readonly ILogger _logger;
        private readonly Action<string, object[]> _logMessage;
        private readonly Stopwatch _stopWatch;
        private readonly string _processName;

        public ILogger Log => _logger;

        private DurationLogger(ILogger logger,
            Action<string, object[]> logMessage)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(logMessage, nameof(logMessage));

            _logger = logger;
            _logMessage = logMessage;

            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        /// <summary>
        /// Creates a new duration logger.
        /// </summary>
        /// <param name="logger">The associated logger.</param>
        /// <param name="processName">The name to identify the executed code being logged.</param>
        /// <param name="logMessage">Delegate called to log the message.</param>
        /// <param name="details">Details to be logged with message.</param>
        public DurationLogger(ILogger logger,
            string processName,
            Action<string, object[]> logMessage) : this(logger, logMessage)
        {
            Check.NotNullOrWhiteSpace(processName, nameof(processName));

            _processName = processName;
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            _logMessage("End Process: {ProcessName}, {ElapsedMs}", new object[] { _processName, _stopWatch.ElapsedMilliseconds });
        }
    }
}
