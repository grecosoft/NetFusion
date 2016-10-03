using NetFusion.Common;
using NetFusion.Common.Exceptions;
using NetFusion.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace NetFusion.Messaging
{
    /// <summary>
    /// An exception that is thrown when there is an exception dispatching a message.
    /// </summary>
    [Serializable]
    public class MessageDispatchException : NetFusionException
    {
        private const string DISPATCH_DETAILS_VALUE = "NetFusionDispatchDetails";

        public MessageDispatchException()
        {

        }

        public MessageDispatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Details = (IDictionary<string, object>)info.GetValue(DISPATCH_DETAILS_VALUE, typeof(IDictionary<string, object>));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(DISPATCH_DETAILS_VALUE, this.Details);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="innerException">The source exception.</param>
        public MessageDispatchException(string errorMessage, Exception innerException) :
                base(errorMessage, innerException)
        {

        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="dispatchInfo">Describes how the message is to be dispatched when published.</param>
        /// <param name="innerException">The source exception.</param>
        public MessageDispatchException(string errorMessage, MessageDispatchInfo dispatchInfo, Exception innerException)
            : base(errorMessage, innerException)
        {
            Check.NotNull(dispatchInfo, nameof(dispatchInfo));

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "InnerErrorMessage", innerException.Message },
                { "DispatchInfo", new
                    {
                        MessageType = dispatchInfo.MessageType.FullName,
                        ConsumerType = dispatchInfo.ConsumerType.FullName,
                        HandlerMethod = dispatchInfo.MessageHandlerMethod.Name
                    }
                }
            };
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="dispatchInfo">Describes how the message is to be dispatched when published.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="innerException">The source exception.</param>
        public MessageDispatchException(string errorMessage, MessageDispatchInfo dispatchInfo, IMessage message, Exception innerException)
            : base(errorMessage, innerException)
        {
            Check.NotNull(dispatchInfo, nameof(dispatchInfo));
            Check.NotNull(message, nameof(message));

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "InnerErrorMessage", innerException.Message },
                { "Message", message },
                { "DispatchInfo", new
                    {
                        MessageType = dispatchInfo.MessageType.FullName,
                        ConsumerType = dispatchInfo.ConsumerType.FullName,
                        HandlerMethod = dispatchInfo.MessageHandlerMethod.Name
                    }
                }
            };
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="dispatchExceptions">The list of dispatch exceptions that occurred
        /// when dispatching the message.</param>
        public MessageDispatchException(string errorMessage, IMessage message,
           IEnumerable<MessageDispatchException> dispatchExceptions) : base(errorMessage)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(dispatchExceptions, nameof(dispatchExceptions));

            this.Details = new Dictionary<string, object>
            {
                { "ErrorMessage", errorMessage },
                { "Message", message },
                { "DispatchExceptions", dispatchExceptions.Select(de => de.Details).ToArray() }
            };
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="message">The message being dispatched.</param>
        /// <param name="innerException">The source exception.</param>
        public MessageDispatchException(string errorMessage, IMessage message, Exception innerException) :
            base(errorMessage, innerException)
        {
            Check.NotNull(message, nameof(message));

            this.Details = new Dictionary<string, object>()
            {
                { "ErrorMessage", errorMessage },
                { "InnerErrorMessage", innerException.Message },
                { "Message", message }
            };
        }
    }
}
