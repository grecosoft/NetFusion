using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Messaging.Routing;

namespace NetFusion.Messaging.InProcess;

/// <summary>
/// Used to specify how in-process messages should be routed to their consumers.
/// A given microservice defines a derived class used to specify the routes.
/// The routing information is read when the microservice is bootstrapped and
/// used at runtime to route messages.
/// </summary>
public abstract class MessageRouter : IMessageRouter
{
    private readonly List<MessageDispatcher> _messageDispatchers = new();

    public IEnumerable<MessageDispatcher> BuildMessageDispatchers()
    {
        OnConfigureRoutes();
        return _messageDispatchers;
    }

    /// <summary>
    /// Override allows microservice to configure how in-process messages
    /// are routed to consumers.
    /// </summary>
    protected abstract void OnConfigureRoutes();
    
    /// <summary>
    /// Message pattern routing a command to a handler not having a result.
    /// </summary>
    /// <param name="route">Delete specified to configure the route.</param>
    /// <typeparam name="TCommand">The type of the command being routed.</typeparam>
    protected void OnCommand<TCommand>(Action<CommandRoute<TCommand>> route)
        where TCommand: ICommand
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var command = new CommandRoute<TCommand>();
        route(command);
        
        ThrowIfMessageRouteExists(command);
        _messageDispatchers.Add(new MessageDispatcher(command));
    }
    
    /// <summary>
    /// Message pattern routing a command to a handler returning a result.
    /// </summary>
    /// <param name="route">Delete specified to configure the route.</param>
    /// <typeparam name="TCommand">The type of the command being routed.</typeparam>
    /// <typeparam name="TResult">The result of the command being routed.</typeparam>
    protected void OnCommand<TCommand, TResult>(Action<CommandRoute<TCommand, TResult>> route)
        where TCommand: ICommand<TResult>
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var command = new CommandRoute<TCommand, TResult>();
        route(command);
        
        ThrowIfMessageRouteExists(command);
        _messageDispatchers.Add(new MessageDispatcher(command));
    }
    
    /// <summary>
    /// Message pattern routing a domain-event to a handler. 
    /// </summary>
    /// <param name="route">Delete specified to configure the route.</param>
    /// <typeparam name="TDomainEvent">The type of the domain-event being routed.</typeparam>
    protected void OnDomainEvent<TDomainEvent>(Action<DomainRoute<TDomainEvent>> route)
        where TDomainEvent: IDomainEvent
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var domainEvent = new DomainRoute<TDomainEvent>();
        route(domainEvent);

        ThrowIfMessageRouteHandlerExists(domainEvent);
        _messageDispatchers.Add(new MessageDispatcher(domainEvent));
    }
    
    /// <summary>
    /// Message pattern routing a domain-event to a handler having additional metadata.
    /// </summary>
    /// <param name="route">Delete specified to configure the route.</param>
    /// <typeparam name="TDomainEvent">The type of the domain-event being routed.</typeparam>
    protected void OnDomainEvent<TDomainEvent>(
        Action<DomainRouteWithMeta<TDomainEvent, DomainEventRouteMeta<TDomainEvent>>> route)
        where TDomainEvent: IDomainEvent
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var domainEvent = new DomainRouteWithMeta<TDomainEvent, DomainEventRouteMeta<TDomainEvent>>();
        route(domainEvent);

        ThrowIfMessageRouteHandlerExists(domainEvent);
        
        var dispatcher = new MessageDispatcher(domainEvent);
        
        var meta = (DomainEventRouteMeta<TDomainEvent>?)domainEvent.RouteMeta;
        if (meta != null)
        {
            dispatcher.IncludeDerivedTypes = meta.IncludedDerivedMessages;
            dispatcher.MessagePredicate = meta.DoesMessageApply;
        }
        
        _messageDispatchers.Add(dispatcher);
    }
    
    /// <summary>
    /// Message pattern routing a query to a handler.
    /// </summary>
    /// <param name="route">Delete specified to configure the route.</param>
    /// <typeparam name="TQuery">The type of the query being routed.</typeparam>
    /// <typeparam name="TResult">The result of the query being routed.</typeparam>
    protected void OnQuery<TQuery, TResult>(Action<QueryRoute<TQuery, TResult>> route)
        where TQuery: IQuery<TResult>
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var query = new QueryRoute<TQuery, TResult>();
        route(query);
        
        ThrowIfMessageRouteExists(query);
        _messageDispatchers.Add(new MessageDispatcher(query));
    }
    
    private void ThrowIfMessageRouteHandlerExists(MessageRoute route)
    {
        MessageDispatcher? dispatchInfo = _messageDispatchers.FirstOrDefault(d =>
            d.MessageType == route.MessageType &&
            d.MessageHandlerMethod == route.HandlerMethodInfo);

        if (dispatchInfo == null) return;

        throw new BootstrapException(
            $"The message of type: {route.MessageType} is already routed to: {dispatchInfo}",
            "message-already-routed-handler");
    }
    
    private void ThrowIfMessageRouteExists(MessageRoute route)
    {
        MessageDispatcher? dispatchInfo = _messageDispatchers.FirstOrDefault(d => d.MessageType == route.MessageType);
        if (dispatchInfo == null) return;
 
        throw new BootstrapException(
            $"The message of type: {route.MessageType} is already routed to: {dispatchInfo}", 
            "message-already-routed");
    }
}