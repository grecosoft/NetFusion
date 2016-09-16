using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using Serilog;
using Serilog.Events;
using System;

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
        public IContainerLogger ForPluginContext<TContext>()
        {
            return ForContext(typeof(TContext));
        }

        public IContainerLogger ForContext(Type contextType)
        {
            var logger = _logger.ForContext(
                SerilogManifest.ContextPropName,
                contextType.AssemblyQualifiedName);

            return new SerilogPluginLogger(logger);
        }

        private void WriteLogMessage(LogEventLevel level, string message, object details)
        {
            if (!_logger.IsEnabled(level))
            {
                return;
            }

            if (details != null)
            {
                _logger.Write(level, message + "{@details}", details.ToIndentedJson());
            }
            else
            {
                _logger.Write(level, message);
            }
        }

        private void WriteLogMessage(LogEventLevel level, string message, Exception ex, object details)
        {
            if (!_logger.IsEnabled(level))
            {
                return;
            }

            if (details != null)
            {
                _logger.Write(level, ex, message + "{@details}", details.ToIndentedJson());
            }
            else
            {
                _logger.Write(level, ex, message);
            }
        }

        public void Verbose(string message, object details)
        {
            WriteLogMessage(LogEventLevel.Verbose, message, details);
        }

        public void Debug(string message, object details)
        {
            WriteLogMessage(LogEventLevel.Debug, message, details);
        }

        public void Info(string message, object details)
        {
            WriteLogMessage(LogEventLevel.Information, message, details);
        }

        public void Warning(string message, object details)
        {
            WriteLogMessage(LogEventLevel.Warning, message, details);
        }

        public void Error(string message, object details)
        {
            WriteLogMessage(LogEventLevel.Error, message, details);
        }

        public void Error(string message, Exception ex, object details)
        {
            WriteLogMessage(LogEventLevel.Error, message, ex, details);
        }
    }
}
