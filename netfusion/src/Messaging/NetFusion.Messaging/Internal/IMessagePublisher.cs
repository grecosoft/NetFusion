namespace NetFusion.Messaging.Internal;

/// <summary>
/// Called when a message is published to allow plugins to customize the publishing of messages.
/// For example the InProcessMessagePublisher dispatches messages locally while another publisher
/// could deliver the messages externally to a central message broker.
/// </summary> 
public interface IMessagePublisher
{
    /// <summary>
    /// Specifies the scope to which publisher sends messages to subscribers.
    /// </summary>
    IntegrationTypes IntegrationType { get; }

    /// <summary>
    /// Publishes a message asynchronously. 
    /// </summary>
    /// <param name="message">The message to be delivered.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task that will be completed when publishing has completed.</returns>
    Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken);
}