using NetFusion.Integration.RabbitMQ.Bus;

namespace NetFusion.Integration.RabbitMQ.Exchanges.Metadata;

/// <summary>
/// Properties defining a RabbitMQ exchange.
/// </summary>
public abstract class ExchangeMeta
{
    /// <summary>
    /// The name of the exchange to be created.
    /// </summary>
    public string ExchangeName { get; set; } = null!;

    /// <summary>
    /// The RabbitMQ type of the exchange to be created.
    /// </summary>
    public string ExchangeType { get; set; } = EasyNetQ.Topology.ExchangeType.Direct;
    
    /// <summary>
    /// Survive server restarts. If this parameter is false, the exchange
    /// will be removed when the server restarts.
    /// </summary>
    public bool IsDurable { get; set; }

    /// <summary>
    /// Delete this exchange when the last queue is unbound.
    /// </summary>
    public bool IsAutoDelete { get; set; }
    
    /// <summary>
    /// Indicates messages published to the queue are of importance
    /// and should be written to the undelivered message exchange.
    /// </summary>
    public string? AlternateExchangeName { get; set; }

    public PublishOptions PublishOptions { get; } = new();

    /// <summary>
    /// Returns the route key for a given domain-event.
    /// </summary>
    /// <param name="domainEvent">The domain-event being published.</param>
    /// <returns></returns>
    internal abstract string GetRouteKey(IMessage domainEvent);
    
    /// <summary>
    /// Determine when the domain-event should be published to the exchange.
    /// </summary>
    /// <param name="domainEvent">The domain-event being published.</param>
    /// <returns>True if the domain-event should be published to the exchange.
    /// Otherwise false.</returns>
    internal abstract bool WhenDomainEvent(IMessage domainEvent);
}

public class ExchangeMeta<TDomainEvent> : ExchangeMeta
    where TDomainEvent : IDomainEvent
{
    private Func<TDomainEvent, string> _routeKey = _ => string.Empty;
    private Predicate<TDomainEvent> _messagePredicate = _ => true;

    /// <summary>
    /// Specifies a delegate called to determine the route key used
    /// for a published domain-event.
    /// </summary>
    /// <param name="domainEvent">The domain-event being published.</param>
    public void RouteKey(Func<TDomainEvent, string> domainEvent)
    { 
        _routeKey = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
    }

    /// <summary>
    /// Specifies a predicate called to determine when a domain-event should
    /// be published to the exchange.  If not specified, all published domain
    /// events are sent to the exchange.
    /// </summary>
    /// <param name="domainEvent">The domain-event being published.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void WhenDomainEvent(Predicate<TDomainEvent> domainEvent)
    {
        _messagePredicate = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
    }
    
    internal override string GetRouteKey(IMessage domainEvent) => _routeKey((TDomainEvent)domainEvent);
    internal override bool WhenDomainEvent(IMessage domainEvent) => _messagePredicate((TDomainEvent)domainEvent);
}