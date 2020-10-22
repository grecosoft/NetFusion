using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Add(LogLevel logLevel, string message, params object[] args)
        { 
        }
    }
}