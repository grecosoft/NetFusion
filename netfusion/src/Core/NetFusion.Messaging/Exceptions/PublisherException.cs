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
        public PublisherException(string message):
            base(message)
        {

        }
        
        public IEnumerable<Exception> ExceptionDetails { get; }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The error message raised when publishing the message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="publisherExceptions">List of exceptions when publishing message to one 
        /// or more publishers.</param>
        public PublisherException(
            string errorMessage,
            IMessage message,
            IEnumerable<PublisherException> publisherExceptions) : base(errorMessage)
        {
            PublishedMessage = message ?? throw new ArgumentNullException(nameof(message));
            ExceptionDetails = publisherExceptions ?? throw new ArgumentNullException(nameof(publisherExceptions));
            
            Details = new Dictionary<string, object>
            {
                { "Message", errorMessage },
                { "PublishExceptionDetails", publisherExceptions.Select(e => e.Details).ToArray() }
            };
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The error message raised when publishing the message.</param>
        /// <param name="detailKey">Identifies the exception details.</param>
        /// <param name="details">Details associated with the exception.</param>
        public PublisherException(
            string errorMessage,
            string detailKey,
            object details): base (errorMessage, detailKey, details)
        {

        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The enricher related error message raised when publishing the message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="enricherExceptions">List of enricher exceptions when publishing message to one
        /// or more publishers.</param>
        public PublisherException(
            string errorMessage,
            IMessage message,
            IEnumerable<EnricherException> enricherExceptions) : base(errorMessage)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            ExceptionDetails = enricherExceptions ?? throw new ArgumentNullException(nameof(enricherExceptions));
            
            Details = new Dictionary<string, object>
            {
                { "Message", errorMessage },
                { "PublishedMessage", message },
                { "PublishExceptionDetails", enricherExceptions.Select(e => e.Details).ToArray() }
            };
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The error message raised when publishing the message.</param>
        /// <param name="eventSource">The entity with associated domain-events.</param>
        /// <param name="publisherExceptions">List exceptions when publishing message to one or more publishers.</param>
        public PublisherException(
            string errorMessage,
            IEventSource eventSource,
            IEnumerable<PublisherException> publisherExceptions) : base(errorMessage)
        {
            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));

            ExceptionDetails = publisherExceptions ?? throw new ArgumentNullException(nameof(publisherExceptions));
                
            Details = new Dictionary<string, object>
            {
                { "Message", errorMessage },
                { "EventSource", eventSource },
                { "PublishExceptionDetails", publisherExceptions.Select(e => e.Details).ToArray() }
            };
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The error message raised when publishing the message.</param>
        /// <param name="message">The messaging being published.</param>
        /// <param name="innerException">The source exception.  If the exception is derived from 
        /// NetFusionException, the detail will be added to this exception's details.</param>
        public PublisherException(
            string errorMessage,
            IMessage message,
            Exception innerException) : base(errorMessage, innerException)
        {
            Details["PublishedMessage"] = message ?? throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="taskItem">The task and associated publisher.</param>
        public PublisherException(TaskListItem<IMessagePublisher> taskItem): 
            base("Error Invoking Publishers.", GetSourceException(taskItem))
        {
            if (taskItem == null) throw new ArgumentNullException(nameof(taskItem));

            var sourceException = GetSourceException(taskItem);

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Publisher", taskItem.Invoker.GetType().FullName }
            };

            if (sourceException is MessageDispatchException dispatchException)
            {
                Details["DispatchDetails"] = dispatchException.Details;
            }
        }

        private static Exception GetSourceException(TaskListItem<IMessagePublisher> taskItem)
        {
            // Get the aggregate inner exception.
            var taskException = taskItem.Task.Exception;
            return taskException?.InnerException;
        }
    }
}
