using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace NetFusion.Base.Logging
{
    /// <summary>
    /// Class representing a log message with a set of optional detailed properties.
    /// </summary>
    public class LogMessage
    {
        private LogMessage() { }
        
        /// <summary>
        /// The log level associated with the message.
        /// </summary>
        public LogLevel LogLevel { get; private set; }
        
        /// <summary>
        /// Log message template containing optional argument values.
        /// </summary>
        public string Message { get; private set; }
        
        /// <summary>
        /// Optional argument values replaced in the message template.
        /// </summary>
        public object[] Args { get; private set; }

        /// <summary>
        /// Set of properties associated with the message used to write
        /// structured event messages.
        /// </summary>
        public List<LogProperty> Properties { get; } = new List<LogProperty>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel">The log level associated with the message.</param>
        /// <param name="message">Log message template containing optional argument values.</param>
        /// <param name="args">Optional argument values replaced in the message template.</param>
        /// <returns>Log message instance.</returns>
        public static LogMessage For(LogLevel logLevel, string message, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(message)) 
                throw new ArgumentException("Message not specified.", nameof(message));
            
            return new LogMessage
            {
                LogLevel = logLevel,
                Message = message,
                Args = args
            };
        }

        /// <summary>
        /// Adds list of log properties to the message written as a structured event log.
        /// </summary>
        /// <param name="properties"> Set of detail properties associated with the message.</param>
        /// <returns>Reference to log message.</returns>
        public LogMessage WithProperties(params LogProperty[] properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));
            
            Properties.AddRange(properties);
            return this;
        }
    }
}