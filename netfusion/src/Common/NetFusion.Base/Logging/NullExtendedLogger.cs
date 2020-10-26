using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Exceptions;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// 
    /// </summary>
    public class NullExtendedLogger : IExtendedLogger
    {
        public void Write<TContext>(LogMessage message)
        {
           
        }

        public void Write<TContext>(IEnumerable<LogMessage> messages)
        {
           
        }

        public void Write<TContext>(LogLevel logLevel, string message, params object[] args)
        {
            
        }

        public void WriteDetails<TContext>(LogLevel logLevel, string message, object details, params object[] args)
        {
            
        }

        public void Error<TContext>(Exception ex, string message, params object[] args)
        {
            
        }

        public void Error<TContext>(NetFusionException ex, string message, params object[] args)
        {
            
        }

        public void ErrorDetails<TContext>(Exception ex, string message, object details, params object[] args)
        {
         
        }
    }
}