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
        private readonly IContainerLogger _logger;
        private readonly Action<string, object> _logMessage;
        private readonly Stopwatch _stopWatch;
        private readonly string _processName;
        private object _completionDetails = new object();
        
        public IContainerLogger Log { get { return _logger; } }

        private DurationLogger(IContainerLogger logger,
            Action<string, object> logMessage)
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
        public DurationLogger(IContainerLogger logger,
            string processName,
            Action<string, object> logMessage,
            object details = null) : this(logger, logMessage)
        {
            Check.NotNullOrWhiteSpace(processName, nameof(processName));

            _processName = processName;
            _logMessage($"Start Process: {processName}", details ?? new { });
        }

        /// <summary>
        /// Details to be logged when the executed code has completed.
        /// </summary>
        /// <param name="details">Details to log with the end process message.</param>
        public void SetCompletionDetails(object details)
        {
            Check.NotNull(details, nameof(details));
            _completionDetails = details;
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            _logMessage("End Process", new
            {
                Named = _processName,
                ElapsedMs = _stopWatch.ElapsedMilliseconds,
                Details = _completionDetails
            });
        }
    }
}
