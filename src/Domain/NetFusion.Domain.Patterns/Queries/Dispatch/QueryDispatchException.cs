﻿using NetFusion.Base.Exceptions;
using NetFusion.Common;
using NetFusion.Domain.Patterns.Queries.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Patterns.Queries.Dispatch
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
        public QueryDispatchException(string message) :
            base(message)
        { }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="innerException">The source exception.  If the exception is derived
        /// from NetFusionException, the details will be added to this exception's details.</param>
        public QueryDispatchException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch error message.</param>
        /// <param name="dispatchInfo">Describes how the query is to be dispatched when dispatched.</param>
        /// <param name="innerException">The source exception.  If the exception is derived from 
        /// NetFusionException, the detail will be added to this exception's details.</param>
        public QueryDispatchException(string message, QueryDispatchInfo dispatchInfo, Exception innerException)
            : base(message, innerException)
        {
            Check.NotNull(dispatchInfo, nameof(dispatchInfo));

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
        /// <param name="errorMessage">Dispatch error message.</param>
        /// <param name="message">The message being dispatched.</param> when dispatching the message.</param>
        public QueryDispatchException(string errorMessage, IEnumerable<QueryFilterException> filterExceptions)
            : base(errorMessage)
        {
            Check.NotNull(filterExceptions, nameof(filterExceptions));

            Details = new Dictionary<string, object>
            {
                { "DispatchExceptions", filterExceptions.Select(de => de.Details).ToArray() }
            };
        }

        /// <summary>
        /// Dispatch Exception.
        /// </summary>
        /// <param name="message">Dispatch exception message.</param>
        /// <param name="details">Details of the query dispatch.</param>
        /// <param name="innerException">The inner received exception.</param>
        public QueryDispatchException(string message, object details, Exception innerException)
            : base (message, details, innerException)
        {

        }
    }
}