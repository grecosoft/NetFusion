namespace NetFusion.Messaging.Routing;

/// <summary>
/// Allows additional metadata to be associated with a given route.
/// </summary>
public interface IRouteMeta
{
    
}

/// <summary>
/// Allows additional metadata to be associated with a given route.
/// </summary>
/// <typeparam name="TMessage">Message type associated with metadata.</typeparam>
public interface IRouteMeta<TMessage> : IRouteMeta
    where TMessage : IMessage
{
    public Type MessageType => typeof(TMessage);
}