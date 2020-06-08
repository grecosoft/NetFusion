using System;
using System.Collections.Generic;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Logging
{
    public enum LogContextType
    {
        PublishedMessage,
        ReceivedMessage
    }
    
    /// <summary>
    /// Class containing information about a message that was either published or received.
    /// </summary>
    public class MessageLog
    {
        /// <summary>
        /// The date and time the log was recorded.
        /// </summary>
        public DateTime DateLogged { get; internal set; }
        
        /// <summary>
        /// The message that was either published or received.
        /// </summary>
        public IMessage Message { get; }
        
        /// <summary>
        /// Specifies the context of the message. 
        /// </summary>
        public LogContextType LogContext { get; }
        
        /// <summary>
        /// The .NET type of the message.
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// Details written by the publisher or receiver of the message.
        /// </summary>
        public IReadOnlyDictionary<string, string> LogDetails { get; }
        
        /// <summary>
        /// Errors written by the publisher or receiver of the message.
        /// </summary>
        public IReadOnlyDictionary<string, string> LogErrors { get; }
        
        /// <summary>
        /// String value that can be used by consumer.
        /// </summary>
        public string Hint { get; private set; }

        public MessageLog(IMessage message, LogContextType logContext)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            MessageType = message.GetType().FullName;
            LogContext = logContext;

            LogDetails = _logDetails;
            LogErrors = _logErrors;
        }

        /// <summary>
        /// Adds a log details message.
        /// </summary>
        /// <param name="name">The name of the associated value.</param>
        /// <param name="value">The value of the named log attribute.</param>
        public void AddLogDetail(string name, string value) => _logDetails[name] = value;
       

        /// <summary>
        /// Adds exception as a log error message.
        /// </summary>
        /// <param name="name">The name of the associated value.</param>
        /// <param name="exception">The exception to be logged.</param>
        public void AddLogError(string name, Exception exception) => _logErrors[name] = exception?.ToString();

        /// <summary>
        /// Adds exception as a log error message.
        /// </summary>
        /// <param name="name">The name of the associated value.</param>
        /// <param name="value">Description of the exception to be logged.</param>
        public void AddLogError(string name, string value) => _logErrors[name] = value;

        /// <summary>
        /// Sets a value that can be used by the consumer.  If the consumer is a web-client,
        /// this value can be used to determine an icon to be displayed for the log.
        /// </summary>
        /// <param name="value"></param>
        public void SentHint(string value)
        {
            Hint = value;
        }
        
        private readonly Dictionary<string, string> _logDetails = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _logErrors = new Dictionary<string, string>();
    }
}