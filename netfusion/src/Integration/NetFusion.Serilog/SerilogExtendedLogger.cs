using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Base.Logging;
using NetFusion.Common.Extensions.Collections;
using SerilogLogger = Serilog.Log;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Events;

namespace NetFusion.Serilog
{
    /// <summary>
    /// Provided extended logging implemented by Serilog without making NetFusion
    /// directly dependent.
    /// </summary>
    public class SerilogExtendedLogger : IExtendedLogger
    {
        public void Log<TContext>(LogMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            var eventLevel = ToSerilogLevel(message.LogLevel);
            if (eventLevel == null) return;
 
            var enrichers = CreatePropertyEnrichers(message).ToArray();
            using (LogContext.Push(enrichers))
            {
                SerilogLogger.ForContext<TContext>().Write(eventLevel.Value, message.Message, message.Args);
            }
        }
        
        public void Log<TContext>(params LogMessage[] messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            
            messages.ForEach(Log<TContext>);
        }
        
        public void Log<TContext>(IEnumerable<LogMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            
            messages.ForEach(Log<TContext>);
        }
        
        public void Log<TContext>(NetFusionException ex, LogMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var eventLevel = ToSerilogLevel(message.LogLevel) ?? LogEventLevel.Error;

            var enrichers = CreatePropertyEnrichers(message).ToList();
            enrichers.Add(new PropertyEnricher("Details", ex.Details, true));
            
            using (LogContext.Push(enrichers.ToArray()))
            {
                SerilogLogger.ForContext<TContext>().Write(eventLevel, ex, message.Message, message.Args);
            }
        }

        public void Log<TContext>(LogLevel logLevel, string message, 
            params object[] args)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            var eventLevel = ToSerilogLevel(logLevel);
            if (eventLevel == null) return;

            SerilogLogger.ForContext<TContext>().Write(eventLevel.Value, message, args);
        }

        public void LogDetails<TContext>(LogLevel logLevel, string message, object details,
            params object[] args)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (details == null) throw new ArgumentNullException(nameof(details));
            
            var eventLevel = ToSerilogLevel(logLevel);
            if (eventLevel == null) return;

            using (LogContext.Push(new PropertyEnricher("Details", GetDetailsToLog(details), true)))
            {
                SerilogLogger.ForContext<TContext>().Write(eventLevel.Value, message, args);
            }
        }

        public void LogError<TContext>(Exception ex, string message, params object[] args)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));

            SerilogLogger.ForContext<TContext>().Error(ex, message, args);
        }

        public void LogErrorDetails<TContext>(Exception ex, string message, object details, 
            params object[] args)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (details == null) throw new ArgumentNullException(nameof(details));

            using (LogContext.Push(new PropertyEnricher("Details", GetDetailsToLog(details), true)))
            {
                SerilogLogger.ForContext<TContext>().Error(ex, message, args);
            }
        }

        public void LogError<TContext>(NetFusionException ex, string message, params object[] args)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            using (LogContext.Push(new PropertyEnricher("Details", ex.Details, true)))
            {
                SerilogLogger.ForContext<TContext>().Error(ex, message, args);
            }
        }
        
        private static IEnumerable<ILogEventEnricher> CreatePropertyEnrichers(LogMessage message) => 
            message.Properties.Select(p => new PropertyEnricher(
                p.Name, 
                GetDetailsToLog(p.Value),
                p.DestructureObjects)
            );
        

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

        private static object GetDetailsToLog(object details) => 
            details is ITypeLog typeLog ? typeLog.Log() : details;
    }
}