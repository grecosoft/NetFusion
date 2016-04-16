using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using Serilog;
using Serilog.Events;

namespace NetFusion.Logging.Serilog
{
    public class SerilogPluginLogger : IContainerLogger
    {
        private readonly ILogger _logger;

        public SerilogPluginLogger(ILogger logger)
        {
            Check.NotNull(logger, nameof(logger), "inner logger not specified");
            _logger = logger;
        }

        public bool IsVerboseLevel
        {
            get { return _logger.IsEnabled(LogEventLevel.Verbose); }
        }

        public bool IsDebugLevel
        {
            get { return _logger.IsEnabled(LogEventLevel.Debug); }
        }

        public void Verbose(string message)
        {
            if (this.IsVerboseLevel)
            {
                _logger.Debug("Container Log: {plugInLog}", message);
            }
        }

        public void Debug(string message)
        {
            if (this.IsDebugLevel)
            {
                _logger.Debug("Container Log: {plugInLog}", message);
            }
        }

        public void Warning(string message)
        {
            if (_logger.IsEnabled(LogEventLevel.Warning))
            {
                _logger.Warning("Container Log: {plugInLog}", message);
            }
        }

        public void Error(string message)
        {
            _logger.Error("{message}", message);
        }
    }
}
