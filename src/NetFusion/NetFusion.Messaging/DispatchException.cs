using NetFusion.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging
{
    /// <summary>
    /// An exception that is thrown when there is an issue dispatching a message.
    /// </summary>
    public class MessageDispatchException : Exception
    {
        public IDictionary<string, object> DispatchDetails { get; }

     
        public MessageDispatchException(string errorMessage, MessageDispatchInfo dispatchInfo, Exception ex)
            : base(errorMessage, ex)
        {
            this.DispatchDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "InnerErrorMessage", ex.Message },
                { "DispatchInfo", new
                    {
                        MessageType = dispatchInfo.MessageType.FullName,
                        ConsumerType = dispatchInfo.ConsumerType.FullName,
                        HandlerMethod = dispatchInfo.MessageHandlerMethod.Name
                    }
                }
            };
        }

        public MessageDispatchException(string errorMessage, IEnumerable<MessageDispatchException> dispatchExceptions) :
            base(errorMessage)
        {
            this.DispatchDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "DispatchExceptions",  dispatchExceptions.Select(de => de.DispatchDetails).ToArray() }
            };
        }

        public MessageDispatchException(string errorMessage, IMessage message,
           IEnumerable<MessageDispatchException> dispatchExceptions) : base(errorMessage)
        {
            this.DispatchDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "SentMessage", message },
                { "DispatchExceptions", dispatchExceptions.Select(de => de.DispatchDetails).ToArray() }
            };
        }

        public MessageDispatchException(string errorMessage, IMessage message,
           IEnumerable<Exception> exceptions) : base(errorMessage)
        {
            this.DispatchDetails = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "SentMessage", message },
                { "DispatchExceptions", exceptions.Select(e => e.Message ) }
            };
        }
        
        public MessageDispatchException(string messageError, IMessage message, Exception innerException) :
            base(messageError, innerException)
        {
            this.DispatchDetails = new Dictionary<string, object>()
            {
                { "ErrorMessage", messageError },
                { "InnerErrorMessage", innerException.Message },
                { "SentMessage", message }
            };
        }

        public MessageDispatchException(string errorMessage, IEventSource eventSource, 
            IEnumerable<MessageDispatchException> dispatchExceptions) :
            base(errorMessage)
        {
            this.DispatchDetails = new Dictionary<string, object>()
            {
                { "ErrorMessage", errorMessage },
                { "EventSource", eventSource },
                { "DispatchDetails", dispatchExceptions.Select(e => e.DispatchDetails).ToArray() }
            };
        }
    }
}
