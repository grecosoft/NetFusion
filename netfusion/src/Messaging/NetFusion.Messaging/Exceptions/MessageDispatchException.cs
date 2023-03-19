using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Messaging.Exceptions;

/// <summary>
/// An exception that is thrown when there is an exception dispatching a message.
/// </summary>
public class MessageDispatchException : NetFusionException
{
    public MessageDispatchException() { }

    /// <summary>
    /// Dispatch exception.
    /// </summary>
    /// <param name="message">Dispatch error message.</param>
    public MessageDispatchException(string message) 
        : base(message) { }

    /// <summary>
    /// Dispatch Exception.
    /// </summary>
    /// <param name="message">Dispatch error message.</param>
    /// <param name="innerException">The source exception.  If the exception is derived
    /// from NetFusionException, the details will be added to this exception's details.</param>
    public MessageDispatchException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// Dispatch Exception.
    /// </summary>
    /// <param name="message">Dispatch error message.</param>
    /// <param name="dispatchInfo">Describes how the message is to be dispatched when published.</param>
    /// <param name="aggregateException">An aggregate exception associated with a task.</param>
    public MessageDispatchException(string message, MessageDispatcher dispatchInfo,
        AggregateException? aggregateException) : base(message, aggregateException)
    {
        if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));

        Details["DispatchInfo"] = new
        {
            MessageType = dispatchInfo.MessageType.FullName,
            ConsumerType = dispatchInfo.ConsumerType.FullName,
            HandlerMethod = dispatchInfo.MessageHandlerMethod.Name
        };
    }

    /// <summary>
    /// Dispatch Exception.
    /// </summary>
    /// <param name="message">Dispatch error message.</param>
    /// <param name="innerException">The source exception that was raised.</param>
    /// <param name="childExceptions">List of associated exceptions.</param>
    public MessageDispatchException(string message, Exception innerException, 
        IEnumerable<NetFusionException> childExceptions) 
        : base(message, innerException)
    {
        SetChildExceptions(childExceptions);
    }
}