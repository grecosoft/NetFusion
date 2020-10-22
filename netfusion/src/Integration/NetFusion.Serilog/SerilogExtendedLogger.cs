using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Events;

namespace NetFusion.Serilog
{
    /// <summary>
    /// Provided extended logging implemented by Serilog.
    /// </summary>
    public class SerilogExtendedLogger : IExtendedLogger
    {
        public void Add(LogLevel logLevel, string message, params object[] args)
        {
            var eventLevel = ToSerilogLevel(logLevel);
            if (eventLevel == null)
            {
                return;
            }
            
            Log.Write(eventLevel.Value, message, args);
        }

        public void Write(LogLevel logLevel, LogMessage message)
        {
            var eventLevel = ToSerilogLevel(logLevel);
            if (eventLevel == null)
            {
                return;
            }

            var enrichers = CreatePropertyEnrichers(message).ToArray();
            using (LogContext.Push(enrichers))
            {
                Log.Write(eventLevel.Value, message.Message, message.Args);
            }
        }

        private static IEnumerable<ILogEventEnricher> CreatePropertyEnrichers(LogMessage message) => 
            message.Properties.Select(p => new PropertyEnricher(p.Name, p.Value, p.DestructureObjects));
        

        private static LogEventLevel? ToSerilogLevel(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                _ => null
            };
        }
    }
}