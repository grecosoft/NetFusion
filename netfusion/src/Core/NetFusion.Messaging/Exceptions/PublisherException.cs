using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IMessage PublishedMessage { get; }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">Generic exception message.</param>
        public PublisherException(string message) : base(message)
        {

        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The error message raised when publishing the message.</param>
        /// <param name="message">The messaging being published.</param>
        /// <param name="innerException">The source exception.  If the exception is derived from 
        /// NetFusionException, the detail will be added to this exception's details.</param>
        public PublisherException(string message, Exception innerException) 
            : base(message, innerException)
        {
            
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">The error message raised when publishing the message.</param>
        /// <param name="detailKey">Identifies the exception details.</param>
        /// <param name="details">Details associated with the exception.</param>
        public PublisherException(string message, string detailKey, object details)
            : base (message, detailKey, details)
        {

        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">The error message raised when publishing the message.</param>
        /// <param name="innerException">The source of the exception.</param>
        /// <param name="publishedMessage">The message being dispatched.</param>
        /// <param name="publisherExceptionDetails">List of exceptions when publishing message to one 
        /// or more publishers.</param>
        public PublisherException(string message, Exception innerException, IMessage publishedMessage,
            IEnumerable<NetFusionException> exceptionDetails) : base(message, innerException)
        {
            PublishedMessage = publishedMessage ?? throw new ArgumentNullException(nameof(publishedMessage));
            ChildExceptions = exceptionDetails ?? throw new ArgumentNullException(nameof(exceptionDetails));

            Details["ExceptionDetails"] = exceptionDetails.Select(e => e.Details).ToArray();
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">The error message raised when publishing the message.</param>
        /// <param name="eventSource">The entity with associated domain-events.</param>
        /// <param name="exceptionDetails">List exceptions when publishing message to one or more publishers.</param>
        public PublisherException(string message, IEventSource eventSource,
            IEnumerable<NetFusionException> exceptionDetails) : base(message)
        {
            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

            ChildExceptions = exceptionDetails ?? throw new ArgumentNullException(nameof(exceptionDetails));

            Details["EventSourceType"] = eventSource.GetType();
            Details["ExceptionDetails"] = exceptionDetails.Select(e => e.Details).ToArray();
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="taskItem">The task and associated publisher.</param>
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
        /// <param name="errorMessage">The enricher related error message raised when publishing the message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="enricherExceptions">List of enricher exceptions when publishing message to one
        /// or more publishers.</param>
        public PublisherException(string message, IEnumerable<EnricherException> enricherExceptions) 
            : base(message)
        {
            ChildExceptions = enricherExceptions ?? throw new ArgumentNullException(nameof(enricherExceptions));

            Details["ExceptionDetails"] = enricherExceptions.Select(e => e.Details).ToArray();
        }
        
        private static Exception GetSourceException(TaskListItem<IMessagePublisher> taskItem)
        {
            // Get the aggregate inner exception.
            var taskException = taskItem.Task.Exception;
            return taskException?.InnerException;
        }
    }
}
