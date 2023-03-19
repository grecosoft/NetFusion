namespace NetFusion.Messaging.Types.Contracts;

/// <summary>
/// Represents a query request that can be dispatched to a consumer.
/// </summary>
public interface IQuery : IMessage
{
    
}

/// <summary>
/// Represents a query request that can be dispatched to a consumer.
/// </summary>
/// <typeparam name="TResult">The type of the result expected by the query.</typeparam>
public interface IQuery<out TResult> : IQuery
{
    /// <summary>
    /// The result of the query's execution.
    /// </summary>
    TResult Result { get; }
}