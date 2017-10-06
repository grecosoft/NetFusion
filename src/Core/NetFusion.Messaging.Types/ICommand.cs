using System;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Represents a message that can have one and only one consumer 
    /// handler.  The handling consumer can associate a result after 
    /// processing the command.  This type of message tells the a 
    /// consumer to complete an action.
    /// </summary>
    public interface ICommand : IMessage
    {
        /// <summary>
        /// The optional result of the command.
        /// </summary>
        object Result { get; }

        /// <summary>
        /// The type of the result associated with the command.
        /// </summary>
        Type ResultType { get; }
         
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
        new TResult Result { get; }
    }
}
