using NetFusion.Common;
using NetFusion.Common.Exceptions;
using NetFusion.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging
{
    /// <summary>
    /// An exception that is raised when there is an error publishing 
    /// a message to one or more registered message publishers.
    /// </summary>
    public class PublisherException : NetFusionException
    {
        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The publish error message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="publisherExceptions">List exceptions when publishing message to
        /// one or more publishers.</param>
        public PublisherException(
            string errorMessage,
            IMessage message,
            IEnumerable<PublisherException> publisherExceptions) : base(errorMessage)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(publisherExceptions, nameof(publisherExceptions));

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "Message", message },
                { "PublishExceptionDetails", publisherExceptions.Select(e => e.Details).ToArray() }
            };
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The publish error message.</param>
        /// <param name="eventSource">The entity with associated domain-events.</param>
        /// <param name="publisherExceptions">List exceptions when publishing message to
        /// one or more publishers.</param>
        public PublisherException(
            string errorMessage,
            IEventSource eventSource,
            IEnumerable<PublisherException> publisherExceptions) : base(errorMessage)
        {
            Check.NotNull(eventSource, nameof(eventSource));
            Check.NotNull(publisherExceptions, nameof(publisherExceptions));

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "EventSource", eventSource },
                { "PublishExceptionDetails", publisherExceptions.Select(e => e.Details).ToArray() }
            };
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="errorMessage">The publish error message.</param>
        /// <param name="message">The messaging being published.</param>
        /// <param name="innerException">The source exception.</param>
        public PublisherException(
            string errorMessage,
            IMessage message,
            Exception innerException) : base(errorMessage, innerException)
        {
            Check.NotNull(message, nameof(message));

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "SentMessage", message },
                { "InnerMessage", innerException.Message }
            };
        }

        /// <summary>
        /// Publisher Exception.
        /// </summary>
        /// <param name="publisherTask">The task and associated publisher.</param>
        public PublisherException(MessagePublisherTask publisherTask): 
            base("Error Invoking Publishers.")
        {
            Check.NotNull(publisherTask, nameof(publisherTask));

            var taskException = publisherTask.Task.Exception;
            var sourceException = taskException.InnerException;

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", sourceException.Message },
                { "Publisher", publisherTask.Publisher.GetType().FullName }
            };

            var dispatchException = sourceException as MessageDispatchException;
            if (dispatchException != null)
            {
                this.Details["DispatchDetails"] = dispatchException.Details;
            }
        }
    }
}
