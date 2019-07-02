using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Simple logger used during the bootstrap process.  This exists since the
    /// ILogger instance is not available until the Service-Provider has been
    /// created from the Service-Collection.  However, there are cases where
    /// logging is needed.  This class should not be used outside of the bootstrap
    /// code executing after ILogger is available.  
    /// </summary>
    public class BootstrapLogger : IBootstrapLogger
    {
        private readonly List<BootstrapLog> _logs = new List<BootstrapLog>();
        private readonly ConsoleColor _defaultBackgroundColor = Console.BackgroundColor;
        private readonly ConsoleColor _defaultForegroundColor = Console.ForegroundColor;

        public IEnumerable<BootstrapLog> Logs => _logs;
            
        public void Add(LogLevel logLevel, string message, params object[] args)
        {
            var log = new BootstrapLog(logLevel, message)
            {
                Args = args
            };
            
            _logs.Add(log);
        }

        public bool HasErrors => _logs.Any(l => l.LogLevel == LogLevel.Error);

        public void WriteToStandardOut()
        {
            foreach (BootstrapLog log in _logs)
            {
                WriteLog(log);
            }
        }

        public void WriteToLogger(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            
            foreach (var log in _logs)
            {
                logger.Log(log.LogLevel, log.Message, log.Args);
            }
        }

        private void WriteLog(BootstrapLog log)
        {
            Console.BackgroundColor = _defaultBackgroundColor;

            switch (log.LogLevel)
            {
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case LogLevel.Warning:
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = _defaultBackgroundColor;
                    break;
            }

            Console.WriteLine($"{log.LogLevel} ==> {log.Message}");
            Console.ForegroundColor = _defaultForegroundColor;
        }
    }
}