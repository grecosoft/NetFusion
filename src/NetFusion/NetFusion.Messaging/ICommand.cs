namespace NetFusion.Messaging
{
    /// <summary>
    /// Represents a message that can have one and only one consumer 
    /// and handler.  The handling consumer can associate a result
    /// after processing the message.  This type of message tells
    /// the consumer to take an action.
    /// </summary>
    public interface ICommand : IMessage
    {
        /// <summary>
        /// Sets the optional result associated with the message.
        /// </summary>
        /// <param name="result">Associated result.</param>
        void SetResult(object result);
    }

    /// <summary>
    /// Represents a message that can have one and only one consumer 
    /// and handler.  The handling consumer can associate a result
    /// after processing the message.  This type of message tells
    /// the consumer to take an action.
    /// </summary>
    /// <typeparam name="TResult">The response set by the consumer that
    /// processed the message.</typeparam>
    public interface ICommand<TResult> : ICommand
    {
        TResult Result { get; }
    }
}
