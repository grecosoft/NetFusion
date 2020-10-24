using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Write(LogLevel logLevel, LogMessage message)
        {

        }

        public void Write(LogLevel logLevel, string message, params object[] args)
        {
           
        }

        public void Error(Exception ex, string message, params object[] args)
        {
          
        }

        public void Error(Exception ex, string message, IDictionary<string, object> details, params object[] args)
        {

        }
    }
}