using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
using Serilog;

namespace NetFusion.Serilog
{
    /// <summary>
    /// Provided extended logging implemented by Serilog.
    /// </summary>
    public class SerilogExtendedLogger : IExtendedLogger
    {
        public void Add(LogLevel logLevel, string message, params object[] args)
        {
            Log.Information(message, args);
        }
    }
}
