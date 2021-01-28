using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Default implementation specified as the extended logger that does
    /// not attempt to log the message.  This is a place holder for the
    /// real implementation used by the host.  See: NetFusion.Serilog.
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Log<TContext>(LogMessage message) => Console.WriteLine(message.ToIndentedJson());
        public void Log<TContext>(params LogMessage[] messages) => Console.WriteLine(messages.ToIndentedJson());
        public void Log<TContext>(IEnumerable<LogMessage> messages) => Console.WriteLine(messages.ToIndentedJson());
        public void Log<TContext>(NetFusionException ex, LogMessage message) { Console.WriteLine(ex.Message); }

        public void Log<TContext>(LogLevel logLevel, string message, params object[] args)
            => Console.WriteLine(new {message, args}.ToIndentedJson());

        public void LogDetails<TContext>(LogLevel logLevel, string message, object details, params object[] args)
            => Console.WriteLine(new { message, details, args }.ToIndentedJson());

        public void LogError<TContext>(Exception ex, string message, params object[] args)
            => Console.WriteLine(new {errorMsg = ex.Message, message, args});

        public void LogErrorDetails<TContext>(Exception ex, string message, object details, params object[] args)
            => Console.WriteLine(new {errorMsg = ex.Message, message, details, args}.ToIndentedJson());

        public void LogError<TContext>(NetFusionException ex, string message, params object[] args)
            => Console.WriteLine(new {errorMsg = ex.Message, message, args}.ToIndentedJson());
    }
}