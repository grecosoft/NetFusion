using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types;

/// <summary>
/// Default implementation representing message that can have one and only one consumer.
/// The handling consumer can associate a result after processing the message.  A command
/// expresses an action that is to take place resulting in a change to an application's state.
/// </summary>
public abstract class Command : ICommand, IMessageWithResult
{
    /// <summary>
    /// List of arbitrary key value pairs associated with the message. 
    /// </summary>
    public IDictionary<string, string> Attributes { get; set; }
        
    protected Command()
    {
        Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
    
    protected virtual Type GetResultType() => typeof(void);
    
    protected IMessageWithResult ResultState => this;
    object? IMessageWithResult.MessageResult { get; set; }
    Type IMessageWithResult.DeclaredResultType => GetResultType();
    
    public void SetResult(object? result)
    {
        // The command result can be null.
        if (result == null) return;
            
        if (!result.GetType().CanAssignTo(ResultState.DeclaredResultType))
        {
            throw new InvalidOperationException(
                $"The command of type: {GetType()} has a declared result type of: {ResultState.DeclaredResultType}. " + 
                $"The type of the result being set is: {result.GetType()} and is not assignable to the " +
                $"command's declared result type of: {ResultState.DeclaredResultType}.");
        }
            
        ResultState.MessageResult = result;
    }
}

/// <summary>
/// Default implementation representing a message that can have one and only one consumer.  
/// The handling consumer can associate a result after processing the message.
/// </summary>
/// <typeparam name="TResult">The response type of the command.</typeparam>
public abstract class Command<TResult> : Command, ICommand<TResult>
{
    protected Command()
    {
        ResultState.MessageResult = default(TResult);
    }

    protected sealed override Type GetResultType() => typeof(TResult);

    /// <summary>
    /// The result of executing the command.
    /// </summary>
    public TResult Result => (TResult)ResultState.MessageResult!;
}