using System;
using System.Collections.Generic;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Base.Validation;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Default implementation of IExtendedLogger writing logs to the console.  This is a
    /// place holder for the real implementation used by the host.  See: NetFusion.Serilog.
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Log<TContext>(LogMessage message) => Console.WriteLine(message);
        public void Log<TContext>(params LogMessage[] messages) => Console.WriteLine(messages);
        public void Log<TContext>(IEnumerable<LogMessage> messages) => Console.WriteLine(messages);
        public void Log<TContext>(NetFusionException ex, LogMessage message) { Console.WriteLine(ex.Message); }

        public void Log<TContext>(LogLevel logLevel, string message, params object[] args)
            => Console.WriteLine(message);

        public void LogDetails<TContext>(LogLevel logLevel, string message, object details, params object[] args)
            => Console.WriteLine(message);

        public void LogError<TContext>(Exception ex, string message, params object[] args)
            => Console.WriteLine(ex.Message);

        public void LogErrorDetails<TContext>(Exception ex, string message, object details, params object[] args)
            => Console.WriteLine(ex.Message);

        public void LogError<TContext>(NetFusionException ex, string message, params object[] args)
            => Console.WriteLine(ex.Message);
    }
}