using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    public class LogMessage
    {
        private LogMessage() { }
        
        public string Message { get; private set; }
        public object[] Args { get; private set; }
        public LogLevel LogLevel { get; private set; }
        public List<LogProperty> Properties { get; } = new List<LogProperty>();

        public static LogMessage For(LogLevel logLevel, string message, params object[] args)
        {
            return new LogMessage
            {
                LogLevel = logLevel,
                Message = message,
                Args = args
            };
        }

        public LogMessage WithProperties(params LogProperty[] properties)
        {
            Properties.AddRange(properties);
            return this;
        }
    }
}