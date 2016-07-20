using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using Serilog;
using Serilog.Events;

namespace NetFusion.Logging.Serilog.Core
{
    /// <summary>
    /// Implements the IContainerLogger interface by delegation to the 
    /// configured Serilog logger.  This allows the core plug-ins to
    /// log to Serilog without have a direct dependency.
    /// </summary>
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

        public bool IsInformationLevel
        {
            get { return _logger.IsEnabled(LogEventLevel.Information); }
        }

        public bool IsWarningLevel
        {
            get { return _logger.IsEnabled(LogEventLevel.Warning); }
        }

        public IContainerLogger ForContext<TContext>()
        {
            var logger = _logger.ForContext("NetFusion-ContextClrType", typeof(TContext).AssemblyQualifiedName);
            return new SerilogPluginLogger(logger);
        }

        public void Verbose(string message)
        {
            if (this.IsVerboseLevel)
            {
                _logger.Verbose("Container Log: {plugInLog}", message);
            }
        }

        public void Debug(string message, object details = null)
        {
            if (this.IsDebugLevel)
            {
                var messageTemp = GetMessageTemplate(message);
                _logger.Debug(messageTemp, details);
            }
        }

        private string GetMessageTemplate(string message)
        {
            return message + ":{@data}";
        }

        public void Information(string message)
        {
            if (this.IsInformationLevel)
            {
                _logger.Information("Container Log: {plugInLog}", message);
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

        public void Fatal(string message)
        {
            _logger.Fatal("{message}", message);
        }
    }
}
