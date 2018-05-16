using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Base.Exceptions;
using NetFusion.Messaging.Core;

namespace NetFusion.Messaging.Exceptions
{
    /// <summary>
    /// An exception that is thrown when there is an exception dispatching a message.
    /// </summary>
    public class MessageDispatchException : NetFusionException
    {
        public MessageDispatchException() { }

        /// <summary>
        /// Dispatch exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        public MessageDispatchException(string message): 
            base(message) { }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="innerException">The source exception.  If the exception is derived
        /// from NetFusionException, the details will be added to this exception's details.</param>
        public MessageDispatchException(string message, Exception innerException) :
            base(message, innerException) { }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="dispatchInfo">Describes how the message is to be dispatched when published.</param>
        /// <param name="innerException">The source exception.  If the exception is derived from 
        /// NetFusionException, the detail will be added to this exception's details.</param>
        public MessageDispatchException(string message, MessageDispatchInfo dispatchInfo, Exception innerException)
            : base(message, innerException)
        {
            if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));
            
            Details["DispatchInfo"] = new
            {
                MessageType = dispatchInfo.MessageType.FullName,
                ConsumerType = dispatchInfo.ConsumerType.FullName,
                HandlerMethod = dispatchInfo.MessageHandlerMethod.Name
            };
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="dispatchExceptions">List of message dispatch exceptions.</param>
        public MessageDispatchException(string errorMessage, IEnumerable<MessageDispatchException> dispatchExceptions) 
            : base(errorMessage)
        {
            if (dispatchExceptions == null) throw new ArgumentNullException(nameof(errorMessage));
            
            Details = new Dictionary<string, object>
            {
                { "DispatchExceptions", dispatchExceptions.Select(de => de.Details).ToArray() }
            };
        }
    }
}
