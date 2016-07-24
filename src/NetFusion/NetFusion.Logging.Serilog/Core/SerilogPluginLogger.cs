using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
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

        // Delegate to Serilog for the current level configuration.
        public bool IsVerboseLevel => _logger.IsEnabled(LogEventLevel.Verbose);
        public bool IsDebugLevel => _logger.IsEnabled(LogEventLevel.Debug);
        public bool IsInfoLevel => _logger.IsEnabled(LogEventLevel.Information);
        public bool IsWarningLevel => _logger.IsEnabled(LogEventLevel.Warning);
        public bool IsErrorLevel => _logger.IsEnabled(LogEventLevel.Error);

        // Returns new instance associated with the specified context.
        public IContainerLogger ForContext<TContext>()
        {
            var logger = _logger.ForContext(
                SerilogManifest.ContextPropName, 
                typeof(TContext).AssemblyQualifiedName);

            return new SerilogPluginLogger(logger);
        }

        private void WriteLogMessage(LogEventLevel level, string message, object details)
        {
            if (_logger.IsEnabled(level))
            {
                _logger.Write(level, message + "{@details}", details.ToIndentedJson());
            }
        }

        public void Verbose(string message, object details = null)
        {
            WriteLogMessage(LogEventLevel.Verbose, message, details);
        }

        public void Debug(string message, object details = null)
        {
            WriteLogMessage(LogEventLevel.Debug, message, details);
        }

        public void Info(string message, object details = null)
        {
            WriteLogMessage(LogEventLevel.Information, message, details);
        }

        public void Warning(string message, object details = null)
        {
            WriteLogMessage(LogEventLevel.Warning, message, details);
        }

        public void Error(string message, object details = null)
        {
            WriteLogMessage(LogEventLevel.Error, message, details);
        }
    }
}
