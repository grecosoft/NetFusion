using NetFusion.Base.Exceptions;
using System;
using System.Collections.Generic;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Exceptions
{
    /// <summary>
    /// An exception that is raised when there is an error publishing a message 
    /// to one or more registered message publishers.
    /// </summary>
    public class PublisherException : NetFusionException
    {
        /// <summary>
        /// Message published resulting in the exception.
        /// </summary>
        public IMessage PublishedMessage { get; }
        
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
            ChildExceptions = exceptionDetails ?? throw new ArgumentNullException(nameof(exceptionDetails));

            AddExceptionDetails(exceptionDetails);
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

            ChildExceptions = exceptionDetails ?? throw new ArgumentNullException(nameof(exceptionDetails));

            Details["EventSourceType"] = eventSource.GetType();
            AddExceptionDetails(exceptionDetails);
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="messagePublisher">Reference to message publisher resulting in an exception.</param>
        /// <param name="aggregateException">The aggregate exception associated with a task.</param>
        public PublisherException(string message, IMessagePublisher messagePublisher,
            AggregateException aggregateException)
            : base(message, aggregateException)
        {
            if (messagePublisher == null) throw new ArgumentNullException(nameof(messagePublisher));
            
            Details["Publisher"] = messagePublisher.GetType().FullName;
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">Message describing the exception.</param>
        /// <param name="enricherExceptions">List of enricher exceptions when publishing message to one
        /// or more publishers.</param>
        public PublisherException(string message, IEnumerable<EnricherException> enricherExceptions) 
            : base(message)
        {
            ChildExceptions = enricherExceptions ?? throw new ArgumentNullException(nameof(enricherExceptions));

            AddExceptionDetails(enricherExceptions);
        }
    }
}
