namespace NetFusion.Messaging.Logging;

/// <summary>
/// Interface implemented by classes and called when message logs are sent to IMessageLogger.
/// The implementation determines where messages should be written.
/// </summary>
public interface IMessageLogSink
{
    /// <summary>
    /// Called by MessageLogger when a message log is recorded.
    /// </summary>
    /// <param name="messageLog">Details about the message.</param>
    /// <returns>Asynchronous task.</returns>
    Task WriteLogAsync(MessageLog messageLog);
}