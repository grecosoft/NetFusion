namespace NetFusion.Messaging.Logging;

/// <summary>
/// Called when messages are published or received.  Applies any needed common
/// functionality and then sends message to all registered IMessageLogSink
/// instances.
/// </summary>
public class MessageLogger : IMessageLogger
{
    private readonly IMessageLogSink[] _messageLogSinks;
        
    public MessageLogger(IEnumerable<IMessageLogSink> messageLogSinks)
    {
        ArgumentNullException.ThrowIfNull(messageLogSinks);

        _messageLogSinks = messageLogSinks.ToArray();
    }

    public bool IsLoggingEnabled => _messageLogSinks.Any();

    public async Task LogAsync(MessageLog messageLog)
    {
        ArgumentNullException.ThrowIfNull(messageLog);

        if (! IsLoggingEnabled)
        {
            return;
        }
            
        messageLog.DateLogged = DateTime.UtcNow;
            
        foreach (var sink in _messageLogSinks)
        {
            await sink.WriteLogAsync(messageLog);
        }
    }
}