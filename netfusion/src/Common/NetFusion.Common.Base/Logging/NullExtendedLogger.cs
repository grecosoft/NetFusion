using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Exceptions;
using NetFusion.Common.Extensions;

namespace NetFusion.Common.Base.Logging;

/// <summary>
/// Default implementation of IExtendedLogger writing logs to the console.  This is a
/// place holder for the real implementation used by the host.  See: NetFusion.Services.Serilog.
/// </summary>
public class NullExtendedLogger : IExtendedLogger
{
    public void Log<TContext>(LogMessage message) => Console.WriteLine(message.Message);
        
    public void Log<TContext>(params LogMessage[] messages) 
        => Console.WriteLine(messages.Select(m => m.Message).ToIndentedJson());
        
    public void Log<TContext>(IEnumerable<LogMessage> messages) 
        => Console.WriteLine(messages.Select(m => m.Message).ToIndentedJson());

    public void Log<TContext>(NetFusionException ex, LogMessage message) 
        => Console.WriteLine(new { ExceptionMsg = ex.Message, message.Message }.ToIndentedJson());

    public void Log<TContext>(LogLevel logLevel, string message, params object[] args)
        => Console.WriteLine(new { logLevel, message }.ToIndentedJson());

    public void LogDetails<TContext>(LogLevel logLevel, string message, object details, params object[] args)
        => Console.WriteLine(new { logLevel, message }.ToIndentedJson());
            
    public void LogError<TContext>(Exception ex, string message, params object[] args)
        => Console.WriteLine(new { ExceptionMsg = ex.Message, message }.ToIndentedJson());

    public void LogErrorDetails<TContext>(Exception ex, string message, object details, params object[] args)
        => Console.WriteLine(new { ExceptionMsg = ex.Message, message }.ToIndentedJson());

    public void LogError<TContext>(NetFusionException ex, string message, params object[] args)
        => Console.WriteLine(new { ExceptionMsg = ex.Message, message }.ToIndentedJson());
}