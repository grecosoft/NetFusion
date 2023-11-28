using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.Bus.Strategies;

/// <summary>
/// Strategy invoked during the publishing of a message and defines how a
/// specific message type is published it an associated service-bus entity. 
/// </summary>
public interface IBusEntityPublishStrategy : IBusEntityStrategy
{
    /// <summary>
    /// Determines if the strategy knows how to publish a specific message type.
    /// </summary>
    /// <param name="messageType">The type of the message being published.</param>
    /// <returns>True if the strategy is associated with the messaged.  Otherwise, false.</returns>
    public bool CanPublishMessageType(Type messageType);
    
    /// <summary>
    /// Called to send the message to its corresponding service-bus defined entity.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Future Result.</returns>
    Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken);
}