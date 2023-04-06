using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types;

/// <summary>
/// An object representing a query dispatched and handled by a consumer.
/// </summary>
public abstract class Query : IQuery, IMessageWithResult
{
    /// <summary>
    /// List of arbitrary key value pairs associated with the message. 
    /// </summary>
    public IDictionary<string, string> Attributes { get; set; }

    protected Query()
    {
        Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
    
    protected abstract Type GetResultType();

    protected IMessageWithResult ResultState => this;
    object? IMessageWithResult.MessageResult { get; set; }
    Type IMessageWithResult.DeclaredResultType => GetResultType();
    
    public void SetResult(object? result)
    {
        // The query result can be null.
        if (result == null) return;
            
        if (!result.GetType().CanAssignTo(ResultState.DeclaredResultType))
        {
            throw new InvalidOperationException(
                $"The query of type: {GetType()} has a declared result type of: {ResultState.DeclaredResultType}. " + 
                $"The type of the result being set is: {result.GetType()} and is not assignable to the " +
                $"query's declared result type of: {ResultState.DeclaredResultType}.");
        }
            
        ResultState.MessageResult = result;
    }
}

/// <summary>
/// An object representing a query, with a declared result type, dispatched 
/// and handled by a consumer.
/// </summary>
/// <typeparam name="TResult">The type of the result expected by the query.</typeparam>
public abstract class Query<TResult> : Query, IQuery<TResult>
{
    protected Query()
    {
        ResultState.MessageResult = default(TResult);
    }
        
    protected sealed override Type GetResultType() => typeof(TResult);

    /// <summary>
    /// The result of the query provided by the consumer.
    /// </summary>
    public TResult Result
    {
        get => (TResult)ResultState.MessageResult!;
        set => ResultState.MessageResult = value;
    }
}