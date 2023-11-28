using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Messaging.Exceptions;

/// <summary>
/// An exception that is raised when there is an error publishing a message 
/// to one or more registered message publishers.
/// </summary>
public class PublisherException : NetFusionException
{
    /// <summary>
    /// Message published resulting in the exception.
    /// </summary>
    public IMessage? PublishedMessage { get; }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    public PublisherException(string message) : base(message)
    {

    }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="innerException">The source exception.  If the exception is derived from 
    /// NetFusionException, the detail will be added to this exception's details.</param>
    public PublisherException(string message, Exception innerException) 
        : base(message, innerException)
    {
            
    }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="detailKey">Identifies the exception details.</param>
    /// <param name="details">Details associated with the exception.</param>
    public PublisherException(string message, string detailKey, object details)
        : base (message, detailKey, details)
    {

    }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="innerException">The source of the exception.</param>
    /// <param name="publishedMessage">The message being dispatched.</param>
    /// <param name="exceptionDetails">List of exceptions when publishing message to one 
    /// or more publishers.</param>
    public PublisherException(string message, Exception innerException,
        IMessage publishedMessage,
        IEnumerable<NetFusionException> exceptionDetails) : base(message, innerException)
    {
        PublishedMessage = publishedMessage ?? throw new ArgumentNullException(nameof(publishedMessage));

        SetChildExceptions(exceptionDetails);
    }
    
    public PublisherException(string message, Exception innerException,
        IEnumerable<NetFusionException> exceptionDetails) : base(message, innerException)
    {
        SetChildExceptions(exceptionDetails);
    }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="eventSource">The entity with associated domain-events.</param>
    /// <param name="exceptionDetails">List exceptions when publishing message to one or more publishers.</param>
    public PublisherException(string message, IEventSource eventSource,
        IEnumerable<NetFusionException> exceptionDetails) : base(message)
    {
        if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

        Details["EventSourceType"] = eventSource.GetType();
        SetChildExceptions(exceptionDetails);
    }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="messagePublisher">Reference to message publisher resulting in an exception.</param>
    /// <param name="aggregateException">The aggregate exception associated with a task.</param>
    public PublisherException(string message, IMessagePublisher messagePublisher,
        AggregateException? aggregateException)
        : base(message, aggregateException)
    {
        if (messagePublisher == null) throw new ArgumentNullException(nameof(messagePublisher));
            
        Details["Publisher"] = messagePublisher.GetType().FullName!;
    }
    
    public PublisherException(string message, IBatchMessagePublisher messagePublisher,
        AggregateException? aggregateException)
        : base(message, aggregateException)
    {
        if (messagePublisher == null) throw new ArgumentNullException(nameof(messagePublisher));
            
        Details["Publisher"] = messagePublisher.GetType().FullName!;
    }
        
    /// <summary>
    /// Publisher Exception.
    /// </summary>
    /// <param name="message">Message describing the exception.</param>
    /// <param name="childExceptions">List of related child exceptions.</param>
    public PublisherException(string message, IEnumerable<NetFusionException> childExceptions) 
        : base(message)
    {
        SetChildExceptions(childExceptions);
    }
}