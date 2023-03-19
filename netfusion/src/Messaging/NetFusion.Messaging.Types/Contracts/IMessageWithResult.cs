namespace NetFusion.Messaging.Types.Contracts;

/// <summary>
/// Interface used internally to access and set the result of a message. 
/// </summary>
public interface IMessageWithResult
{
    /// <summary>
    /// The optional result of the command.
    /// </summary>
    object? MessageResult { get; set; }

    /// <summary>
    /// The type of the result associated with the command.
    /// </summary>
    Type DeclaredResultType { get; }
         
    /// <summary>
    /// Sets the optional result associated with the message.
    /// </summary>
    /// <param name="result">Associated result.</param>
    void SetResult(object? result);
}