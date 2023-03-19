using System.Linq.Expressions;

namespace NetFusion.Messaging.Routing;

/// <summary>
/// Message pattern used to route a command having a result to a consumer.
/// </summary>
/// <typeparam name="TCommand">Type of command being routed.</typeparam>
/// <typeparam name="TResult">Result type of the command.</typeparam>
public class CommandRoute<TCommand, TResult> : MessageRoute
    where TCommand: ICommand<TResult>
{
    public CommandRoute(): base(typeof(TCommand), typeof(TResult)) { }

    /// <summary>
    /// Routes command to a consumer's handler returning a result.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, TResult>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }

    /// <summary>
    /// Routes command to a consumer's asynchronous handler returning a result.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, Task<TResult>>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
    
    /// <summary>
    /// Routes command to a consumer's asynchronous handler returning a result that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, CancellationToken, Task<TResult>>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
}

/// <summary>
/// Message pattern used to route a command not having a result to a consumer.
/// </summary>
/// <typeparam name="TCommand">Type of command being routed.</typeparam>
public class CommandRoute<TCommand> : MessageRoute
    where TCommand: ICommand
{
    public CommandRoute(): base(typeof(TCommand)) { }
    
    /// <summary>
    /// Routes command to a consumer's handler.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Action<TCommand>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }

    /// <summary>
    /// Routes command to a consumer's asynchronous handler.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, Task>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
    
    /// <summary>
    /// Routes command to a consumer's asynchronous handler that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, CancellationToken, Task>>> consumer)
    {
        SetConsumer<TConsumer>(consumer);
    }
}

/// <summary>
/// Message pattern used to route a command having a result and associated metadata to a consumer.
/// </summary>
/// <typeparam name="TCommand">Type of command being routed.</typeparam>
/// <typeparam name="TResult">Result type of the command.</typeparam>
/// <typeparam name="TRouteMeta">The type of metadata associated with the route.</typeparam>
public class CommandRouteWithMeta<TCommand, TResult, TRouteMeta> : MessageRoute
    where TCommand: ICommand<TResult>
    where TRouteMeta : IRouteMeta<TCommand>, new()
{
    public new TRouteMeta RouteMeta { get; }
    
    public CommandRouteWithMeta() : base(typeof(TCommand), typeof(TResult))
    {
        RouteMeta = new TRouteMeta();
    }
    
    /// <summary>
    /// Routes command to a consumer's handler with additional route metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <param name="routeMeta">Action used to configure the metadata.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, TResult>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }

    /// <summary>
    /// Routes command to a consumer's asynchronous handler with additional route metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <param name="routeMeta">Action used to configure the metadata.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, Task<TResult>>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
    
    /// <summary>
    /// Routes command to a consumer's asynchronous handler with additional route metadata that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <param name="routeMeta">Action used to configure the metadata.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, CancellationToken, Task<TResult>>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
}

/// <summary>
/// Message pattern used to route a command not having a result with associated metadata to a consumer.
/// </summary>
/// <typeparam name="TCommand">Type of command being routed.</typeparam>
/// <typeparam name="TRouteMeta">The type of metadata associated with the route.</typeparam>
public class CommandRouteWithMeta<TCommand, TRouteMeta> : MessageRoute
    where TCommand: ICommand
    where TRouteMeta: IRouteMeta<TCommand>, new()
{
    public new TRouteMeta RouteMeta { get; }

    public CommandRouteWithMeta() : base(typeof(TCommand))
    {
        RouteMeta = new TRouteMeta();
    }
    
    /// <summary>
    /// Routes command to a consumer's handler with additional route metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <param name="routeMeta">Action used to configure the metadata.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Action<TCommand>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }

    /// <summary>
    /// Routes command to a consumer's asynchronous handler with additional route metadata.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <param name="routeMeta">Action used to configure the metadata.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, Task>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
    
    /// <summary>
    /// Routes command to a consumer's asynchronous handler with additional route metadata that can be canceled.
    /// </summary>
    /// <param name="consumer">The consumer's method to handle the command.</param>
    /// <param name="routeMeta">Action used to configure the metadata.</param>
    /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
    public void ToConsumer<TConsumer>(
        Expression<Func<TConsumer, Func<TCommand, CancellationToken, Task>>> consumer,
        Action<TRouteMeta> routeMeta)
    {
        SetConsumer<TConsumer>(consumer);
        ConfigureRouteMeta(RouteMeta, routeMeta);
    }
}
