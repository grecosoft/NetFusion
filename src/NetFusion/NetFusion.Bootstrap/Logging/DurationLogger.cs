using System;
using System.Diagnostics;

namespace NetFusion.Bootstrap.Logging
{
    public class DurationLogger : IDisposable
    {
        private readonly IContainerLogger _logger;
        private readonly string _processName;

        private readonly Stopwatch _stopWatch;
        private readonly Action<string, object> _logMessage;
        private object _completionDetails = new object();
        
        public IContainerLogger Log { get; }

        private DurationLogger(IContainerLogger logger,
            Action<string, object> logMessage)
        {
            this.Log = logger;
            _logMessage = logMessage;

            _logger = logger;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
        }

        public DurationLogger(IContainerLogger logger, 
            string processName,
            Action<string, object> logMessage) : this(logger, logMessage)
        {
            _processName = processName;
            _logMessage($"Start Process: {processName}", new { });
        }

        public DurationLogger(IContainerLogger logger,
            string processName,
            Action<string, object> logMessage,
            object details) : this(logger, logMessage)
        {
            _processName = processName;
            _logMessage($"Start Process: {processName}", details);
        }

        public void SetCompletionDetails(object details)
        {
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
