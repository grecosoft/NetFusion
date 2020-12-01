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
        public IEnumerable<Exception> ExceptionDetails { get; }
        
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
        /// <param name="errorMessage">The error message raised when publishing the message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="publisherExceptionDetails">List of exceptions when publishing message to one 
        /// or more publishers.</param>
        public PublisherException(string message, IMessage publishedMessage,
            IEnumerable<PublisherException> exceptionDetails) : base(message)
        {
            PublishedMessage = publishedMessage ?? throw new ArgumentNullException(nameof(publishedMessage));
            ExceptionDetails = exceptionDetails ?? throw new ArgumentNullException(nameof(exceptionDetails));

            Details["ExceptionDetails"] = exceptionDetails.Select(e => e.Details).ToArray();
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">The error message raised when publishing the message.</param>
        /// <param name="eventSource">The entity with associated domain-events.</param>
        /// <param name="exceptionDetails">List exceptions when publishing message to one or more publishers.</param>
        public PublisherException(string message, IEventSource eventSource,
            IEnumerable<PublisherException> exceptionDetails) : base(message)
        {
            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

            ExceptionDetails = exceptionDetails ?? throw new ArgumentNullException(nameof(exceptionDetails));

            Details["EventSourceType"] = eventSource.GetType();
            Details["ExceptionDetails"] = exceptionDetails.Select(e => e.Details).ToArray();
        }
        
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="taskItem">The task and associated publisher.</param>
        public PublisherException(TaskListItem<IMessagePublisher> taskItem)
            : base("Error Invoking Publisher.", GetSourceException(taskItem), false)
        {
            if (taskItem == null) throw new ArgumentNullException(nameof(taskItem));

            Details["Publisher"] = taskItem.Invoker.GetType().FullName;
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
            ExceptionDetails = enricherExceptions ?? throw new ArgumentNullException(nameof(enricherExceptions));

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
