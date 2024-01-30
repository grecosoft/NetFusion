using System.Linq.Expressions;

namespace NetFusion.Messaging.Routing;

/// <summary>
/// Message pattern used to route a query to a consumer.
/// </summary>
/// <typeparam name="TQuery">Type of query being routed.</typeparam>
/// <typeparam name="TResult">Result type of the query.</typeparam>
public class QueryRoute<TQuery, TResult>() : MessageRoute(typeof(TQuery), typeof(TResult))
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Routes query to a consumer's handler.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the query.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TQuery, TResult>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }

    /// <summary>
    /// Routes query to a consumer's asynchronous handler.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the query.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TQuery, Task<TResult>>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
    
    /// <summary>
    /// Routes query to a consumer's asynchronous handler that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the query.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TQuery, CancellationToken, Task<TResult>>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
}

/// <summary>
/// Message pattern used to route a query having associated metadata to a consumer.
/// </summary>
/// <typeparam name="TQuery">Type of query being routed.</typeparam>
/// <typeparam name="TResult">Result type of the query.</typeparam>
/// <typeparam name="TRouteMeta">The type of metadata associated with the route.</typeparam>
public class QueryRouteWithMeta<TQuery, TResult, TRouteMeta>() : MessageRoute(typeof(TQuery), typeof(TResult))
    where TQuery : IQuery<TResult>
    where TRouteMeta : IRouteMeta<TQuery>, new()
{
    private new TRouteMeta RouteMeta { get; } = new();

    /// <summary>
    /// Routes query to a consumer's handler containing additional metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the query.</param>
    /// <param name="routeMeta"></param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TQuery, TResult>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }

    /// <summary>
    /// Routes query to a consumer's asynchronous handler containing additional metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the query.</param>
    /// <param name="routeMeta"></param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TQuery, Task<TResult>>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
    
    /// <summary>
    /// Routes query to a consumer's asynchronous handler that can be canceled containing additional metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the query.</param>
    /// <param name="routeMeta"></param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TQuery, CancellationToken, Task<TResult>>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
}

