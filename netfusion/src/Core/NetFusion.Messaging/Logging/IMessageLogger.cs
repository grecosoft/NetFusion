using System.Threading.Tasks;

namespace NetFusion.Messaging.Logging
{
    /// <summary>
    /// Called by message processing code to log message publishing and subscribing information.
    /// Implementation responsible dispatching a log message to all registered IMessageLogSink
    /// implementations.  This logging is optional and in addition to the detailed messaging
    /// logs written to ILogger.  This allows higher-level logs to be correlated.
    /// </summary>
    public interface IMessageLogger
    {
        /// <summary>
        /// Logging is enabled if there is one or more registered IMessageLogSink services.
        /// </summary>
        bool IsLoggingEnabled { get; }
        
        /// <summary>
        /// Called to record a log.  The log is sent to all registered IMessageLogSink services.
        /// </summary>
        /// <param name="messageLog">Contains details of how the message was published or handled.</param>
        /// <returns>Task indicating a future result.</returns>
        Task LogAsync(MessageLog messageLog);
    }
}