using NetFusion.Messaging.Routing;

namespace NetFusion.Messaging.InProcess;

/// <summary>
/// Metadata associated with a domain-event route determining how
/// it is routed to its consumer.
/// </summary>
/// <typeparam name="TDomainEvent">The type of the domain-event.</typeparam>
public class DomainEventRouteMeta<TDomainEvent> : IRouteMeta<TDomainEvent>
    where TDomainEvent: IDomainEvent
{
    private Predicate<TDomainEvent>? _predicate;
    
    /// <summary>
    /// Indicates that the consumer's handler should be called for all domain-events
    /// deriving from TDomainEvent.  This is false by default.
    /// </summary>
    public bool IncludedDerivedMessages { get; set; }
    
    internal bool DoesMessageApply(IMessage message) => _predicate == null || _predicate.Invoke((TDomainEvent)message);

    /// <summary>
    /// Used to specify a predicate invoked when a domain-event is published
    /// to determine if the handler applies.
    /// </summary>
    /// <param name="domainEvent">The published domain-event.</param>
    public void When(Predicate<TDomainEvent> domainEvent)
    {
        _predicate = domainEvent;
    }
}