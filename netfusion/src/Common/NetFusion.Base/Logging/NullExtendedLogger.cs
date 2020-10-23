using System;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Add(LogLevel logLevel, string message, params object[] args)
        { 
        }

        public void Write(LogLevel logLevel, LogMessage message)
        {

        }

        public void Error(Exception ex, string message, params object[] args)
        {
          
        }
    }
}