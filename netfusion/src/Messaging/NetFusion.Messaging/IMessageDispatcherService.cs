namespace NetFusion.Messaging;

public interface IMessageDispatcherService
{
    /// <summary>
    /// Service used to dispatch messages that are received outside of the
    /// scope associated with a given web request such as a message received
    /// from a message bus.
    /// </summary>
    /// <param name="dispatcher">The dispatcher to be used to deliver message.</param>
    /// <param name="message">The message to be dispatched.</param>
    /// <param name="cancellationToken">The cancellation token. </param>
    /// <returns>Optional message response.</returns>
    Task<object?> InvokeDispatcherInNewLifetimeScopeAsync(MessageDispatcher dispatcher,
        IMessage message,
        CancellationToken cancellationToken = default);

}