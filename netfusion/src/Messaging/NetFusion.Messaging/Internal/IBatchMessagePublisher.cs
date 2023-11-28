namespace NetFusion.Messaging.Internal;

/// <summary>
/// Optional interface implemented by a message publisher supporting
/// sending batches of domain-events.
/// </summary>
public interface IBatchMessagePublisher
{
    /// <summary>
    /// Publishes a list of domain-events in batch.
    /// </summary>
    /// <param name="domainEvents">List of domain-events to be published.</param>
    /// <param name="cancellationToken">Cancellation token used to cancel async task.</param>
    /// <returns>Future Task Result.</returns>
    Task PublicDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken);
}