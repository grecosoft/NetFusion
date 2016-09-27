using NetFusion.Common.Extensions;
using NetFusion.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging
{
    /// <summary>
    /// An exception that is raised when there is an error publishing 
    /// an event.
    /// </summary>
    public class PublisherException : Exception
    {
        public IDictionary<string, object> PublishDetails { get; }

        public string Details => this.PublishDetails.ToIndentedJson();
      
        public PublisherException(
            string errorMessage,
            IMessage message,
            IEnumerable<PublisherException> publisherExceptions) : base(errorMessage)
        {
            this.PublishDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "SentMessage", message },
                { "PublishDetails", publisherExceptions.Select(e => e.PublishDetails).ToArray() }
            };
        }

        public PublisherException(
            string errorMessage,
            IEventSource eventSource,
            IEnumerable<PublisherException> publisherExceptions) : base(errorMessage)
        {
            this.PublishDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "EventSource", eventSource },
                { "PublishDetails", publisherExceptions.Select(e => e.PublishDetails).ToArray() }
            };
        }

        public PublisherException(
            string errorMessage,
            IMessage message,
            Exception innerException) : base(errorMessage)
        {
            this.PublishDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "SentMessage", message },
                { "InnerMessage", innerException.Message }
            };
        }

        public PublisherException(MessagePublisherTask publisherTask)
        {
            var taskException = publisherTask.Task.Exception;
            var sourceException = taskException.InnerException;

            this.PublishDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", sourceException.Message },
                { "Publisher", publisherTask.Publisher.GetType().FullName }
            };

            var dispatchException = sourceException as MessageDispatchException;
            if (dispatchException != null)
            {
                this.PublishDetails["DispatchDetails"] = dispatchException.DispatchDetails;
            }
        }

        public PublisherException(
            string errorMessage, 
            IMessagePublisher publisher, 
            Exception innerException) : base(errorMessage)
        {
            this.PublishDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "InnerErrorMessage", innerException.Message },
                { "Publisher", publisher.GetType().FullName }
            };
        }
    }
}
