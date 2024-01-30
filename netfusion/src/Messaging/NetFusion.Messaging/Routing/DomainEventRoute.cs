using System.Linq.Expressions;

namespace NetFusion.Messaging.Routing;

/// <summary>
/// Message pattern used to route a domain-event to a consumer.
/// </summary>
/// <typeparam name="TDomainEvent">Type of domain-event being routed.</typeparam>
public class DomainEventRoute<TDomainEvent>() : MessageRoute(typeof(TDomainEvent))
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Routes domain-event to a consumer's handler.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the domain-event.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Action<TDomainEvent>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }

    /// <summary>
    /// Routes domain-event to a consumer's asynchronous handler.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the domain-event.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TDomainEvent, Task>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
    
    /// <summary>
    /// Routes domain-event to a consumer's asynchronous handler that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the domain-event.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TDomainEvent, CancellationToken, Task>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
}

/// <summary>
/// Message pattern used to route a domain-event having associated metadata to a consumer.
/// </summary>
/// <typeparam name="TDomainEvent">Type of domain-event being routed.</typeparam>
/// <typeparam name="TRouteMeta">The type of metadata associated with the route.</typeparam>
public class DomainRouteWithMeta<TDomainEvent, TRouteMeta>() : MessageRoute(typeof(TDomainEvent))
    where TDomainEvent : IDomainEvent
    where TRouteMeta : IRouteMeta<TDomainEvent>, new()
{
    public new TRouteMeta RouteMeta { get; } = new();

    /// <summary>
    /// Routes domain-event to a consumer's handler with additional route metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the domain-event.</param>
    /// <param name="routeMeta"></param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Action<TDomainEvent>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }

    /// <summary>
    /// Routes domain-event to a consumer's asynchronous handler with additional route metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the domain-event.</param>
    /// <param name="routeMeta"></param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TDomainEvent, Task>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
    
    /// <summary>
    /// Routes domain-event to a consumer's asynchronous handler with additional route metadata
    /// that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the domain-event.</param>
    /// <param name="routeMeta"></param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TDomainEvent, CancellationToken, Task>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
}
