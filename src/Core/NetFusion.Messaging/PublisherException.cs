using NetFusion.Base.Exceptions;
using NetFusion.Common;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging
{
    /// <summary>
    /// An exception that is raised when there is an error publishing a message 
    /// to one or more registered message publishers.
    /// </summary>
    public class PublisherException : NetFusionException
    {
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="message">Generic exception message.</param>
        public PublisherException(string message):
            base(message)
        {

        }

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
            Check.NotNull(message, nameof(message));
            Check.NotNull(publisherExceptions, nameof(publisherExceptions));

            Details = new Dictionary<string, object>
            {
                { "Message", errorMessage },
                { "PublishedMessage", message },
                { "PublishExceptionDetails", publisherExceptions.Select(e => e.Details).ToArray() }
            };
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
            Check.NotNull(message, nameof(message));
            Check.NotNull(enricherExceptions, nameof(enricherExceptions));

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
            Check.NotNull(eventSource, nameof(eventSource));
            Check.NotNull(publisherExceptions, nameof(publisherExceptions));

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
            Check.NotNull(message, nameof(message));

            Details["PublishedMessage"] = message;
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="futureResult">The task and associated publisher.</param>
        public PublisherException(TaskListItem<IMessagePublisher> futureResult): 
            base("Error Invoking Publishers.")
        {
            Check.NotNull(futureResult, nameof(futureResult));

            var taskException = futureResult.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Publisher", futureResult.Invoker.GetType().FullName }
            };

            if (sourceException is MessageDispatchException dispatchException)
            {
                Details["DispatchDetails"] = dispatchException.Details;
            }
        }
    }
}
