using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Base.Logging;
using NetFusion.Common.Extensions.Collections;
using Serilog;
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
        public void Write<TContext>(LogMessage message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            var eventLevel = ToSerilogLevel(message.LogLevel);
            if (eventLevel == null) return;
 
            var enrichers = CreatePropertyEnrichers(message).ToArray();
            using (LogContext.Push(enrichers))
            {
                Log.ForContext<TContext>().Write(eventLevel.Value, message.Message, message.Args);
            }
        }
        
        public void Write<TContext>(params LogMessage[] messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            
            messages.ForEach(Write<TContext>);
        }
        
        public void Write<TContext>(IEnumerable<LogMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            
            messages.ForEach(Write<TContext>);
        }

        public void Write<TContext>(LogLevel logLevel, string message, 
            params object[] args)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            var eventLevel = ToSerilogLevel(logLevel);
            if (eventLevel == null) return;

            Log.ForContext<TContext>().Write(eventLevel.Value, message, args);
        }

        public void WriteDetails<TContext>(LogLevel logLevel, string message, object details, 
            params object[] args)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (details == null) throw new ArgumentNullException(nameof(details));
            
            var eventLevel = ToSerilogLevel(logLevel);
            if (eventLevel == null) return;

            using (LogContext.Push(new PropertyEnricher("Details", GetDetailsToLog(details), true)))
            {
                Log.ForContext<TContext>().Write(eventLevel.Value, message, args);
            }
        }

        public void Error<TContext>(Exception ex, string message, params object[] args)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));

            Log.ForContext<TContext>().Error(ex, message, args);
        }

        public void ErrorDetails<TContext>(Exception ex, string message, object details, 
            params object[] args)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (details == null) throw new ArgumentNullException(nameof(details));

            using (LogContext.Push(new PropertyEnricher("Details", GetDetailsToLog(details), true)))
            {
                Log.ForContext<TContext>().Error(ex, message, args);
            }
        }

        public void Error<TContext>(NetFusionException ex, string message, params object[] args)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (message == null) throw new ArgumentNullException(nameof(message));
            
            using (LogContext.Push(new PropertyEnricher("Details", ex.Details, true)))
            {
                Log.ForContext<TContext>().Error(ex, message, args);
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