using Microsoft.Extensions.Logging;
using NetFusion.Common.Base;
using NetFusion.Common.Base.Logging;
using Serilog.Core;
using Serilog.Events;

namespace NetFusion.Services.Serilog;

/// <summary>
/// Can be registered to switch the minimum loglevel at runtime
/// when using Serilog.
/// </summary>
public class LogLevelControl : ILogLevelControl
{
    public LoggingLevelSwitch Switch { get; } = new();
        
    public string SetMinimumLevel(LogLevel logLevel)
    {
        LogEventLevel? eventLevel = SerilogExtendedLogger.ToSerilogLevel(logLevel);
        if (eventLevel == null)
        {
            NfExtensions.Logger.Log<LogLevelControl>(LogLevel.Error, 
                "Specified log level {LogLevel} is invalid", logLevel);
                
            return Switch.MinimumLevel.ToString();
        }
            
        NfExtensions.Logger.Log<LogLevelControl>(LogLevel.Information, 
            "Serilog minimum log level set to {LogLevel}", eventLevel);

        Switch.MinimumLevel = eventLevel.Value;
        return eventLevel.ToString()!;
    }
}