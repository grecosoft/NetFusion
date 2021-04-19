using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Base.Exceptions;
using NetFusion.Messaging.Internal;

namespace NetFusion.Messaging.Exceptions
{
    /// <summary>
    /// An exception that is thrown when there is an exception dispatching a query to a consumer.
    /// </summary>
    public class QueryDispatchException : NetFusionException
    {
        public QueryDispatchException() { }

        /// <summary>
        /// Dispatch exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        public QueryDispatchException(string message) : base(message)
        {
            
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="innerException">The source exception.  If the exception is derived
        /// from NetFusionException, the details will be added to this exception's details.</param>
        public QueryDispatchException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="dispatchInfo">Describes how the query is to be dispatched when dispatched.</param>
        /// <param name="innerException">The source exception.  If the exception is derived from 
        /// NetFusionException, the detail will be added to this exception's details.</param>
        public QueryDispatchException(string message, QueryDispatchInfo dispatchInfo,
            Exception innerException)
            : base(message, innerException)
        {
            if (dispatchInfo == null) throw new ArgumentNullException(nameof(dispatchInfo));

            Details["DispatchInfo"] = new
            {
                QueryType = dispatchInfo.QueryType.FullName,
                ConsumerType = dispatchInfo.ConsumerType.FullName,
                HandlerMethod = dispatchInfo.HandlerMethod.Name
            };
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="filterExceptions">List of exceptions for failed query filters.</param>
        public QueryDispatchException(string message, IEnumerable<QueryFilterException> filterExceptions)
            : base(message)
        {
            if (filterExceptions == null) throw new ArgumentNullException(nameof(filterExceptions));

            Details["DispatchExceptions"] = filterExceptions.Select(de => de.Details).ToArray();
        }
    }
}