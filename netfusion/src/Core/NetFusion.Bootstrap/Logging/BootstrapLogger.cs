using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace NetFusion.Bootstrap.Logging
{
    /// <summary>
    /// Simple logger used during the bootstrap process.  This exists since the
    /// ILogger instance is not available until the Service-Provider has been
    /// created from the Service-Collection.  However, there are cases were where
    /// logging is needed.  This class should not be used outside of the bootstrap
    /// code executing after ILogger is available.  
    /// </summary>
    public class BootstrapLogger : IBootstrapLogger
    {
        private readonly List<BootstrapLog> _logs = new List<BootstrapLog>();
        private ConsoleColor DefaultBackgroundColor = Console.BackgroundColor;
        private ConsoleColor DefaultForegroundColor = Console.ForegroundColor;

        public BootstrapLog[] Logs => _logs.ToArray();
            
        public void Add(LogLevel logLevel, string message, params object[] args)
        {
            var log = new BootstrapLog(logLevel, message)
            {
                Args = args
            };
            
            _logs.Add(log);
        }

        public void WriteToStandardOut()
        {
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            
            Console.WriteLine("--------------------Bootstrap Log--------------------");

            foreach (BootstrapLog log in _logs)
            {
                WriteLog(log);
            }
        }

        private void WriteLog(BootstrapLog log)
        {
            Console.BackgroundColor = DefaultBackgroundColor;

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
                    Console.ForegroundColor = DefaultBackgroundColor;
                    break;
            }

            Console.WriteLine($"{log.LogLevel} ==> {log.Message}");
            Console.ForegroundColor = DefaultForegroundColor;
        }
    }
}