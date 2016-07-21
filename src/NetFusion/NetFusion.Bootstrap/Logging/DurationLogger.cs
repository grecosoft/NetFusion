using System;
using System.Diagnostics;

namespace NetFusion.Bootstrap.Logging
{
    public class DurationLogger : IDisposable
    {
        private readonly IContainerLogger _logger;
        private readonly string _processName;
        private readonly Stopwatch _stopWatch;
        
        public IContainerLogger Log { get; }

     
        public DurationLogger(IContainerLogger logger, string processName)
        {
            this.Log = logger;

            _logger = logger;
            _processName = processName;
            _stopWatch = new Stopwatch();

            _logger.Debug("Start Process", new { Named = _processName });
            _stopWatch.Start();
        }

        public void Dispose()
        {
            _stopWatch.Stop();
            _logger.Debug("End Process", new
            {
                Named = _processName,
                ElapsedMs = _stopWatch.ElapsedMilliseconds
            });
        }
    }
}
