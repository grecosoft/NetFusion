using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    public class BootstrapLog
    {
        public LogLevel LogLevel { get; }
        public string Message { get; }

        public object[] Args { get; set; }

        public BootstrapLog(LogLevel logLevel, string message)
        {
            LogLevel = logLevel;
            Message = message;
        }
    }
}