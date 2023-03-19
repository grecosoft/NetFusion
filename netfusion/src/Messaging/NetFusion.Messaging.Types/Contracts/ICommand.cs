namespace NetFusion.Messaging.Types.Contracts;

/// <summary>
/// Represents a message that can have one and only one consumer.  The handling consumer
/// can associate a result after processing the message. A command expresses an action that
/// is to take place resulting in a change to an application's state.
/// </summary>
public interface ICommand : IMessage
{
        
}

/// <summary>
/// Represents a message that can have one and only one consumer.  The handling consumer
/// can associate a result after processing the message.  This type of message tells the
/// consumer to take an action.
/// </summary>
/// <typeparam name="TResult">The response set by the consumer that processed the message.</typeparam>
public interface ICommand<out TResult> : ICommand
{
    TResult Result { get; }
}