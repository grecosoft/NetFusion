using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types;

/// <summary>
/// Generic message when not classified as a DomainEvent or Command.
/// Often used to model asynchronous responses to commands.
/// </summary>
public class Message : IMessage
{
    protected Message()
    {
        Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
        
    /// <summary>
    /// List of arbitrary key value pairs associated with the message. 
    /// </summary>
    public IDictionary<string, string> Attributes { get; set; }
}