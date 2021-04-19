using System;
using NetFusion.Base.Exceptions;
using NetFusion.Messaging.Filters;

namespace NetFusion.Messaging.Exceptions
{
    /// <summary>
    /// Contains information about a query filter that threw an exception
    /// when applied.
    /// </summary>
    public class QueryFilterException : NetFusionException
    {
        protected QueryFilterException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="taskItem">Task result containing the exception.</param>
        public QueryFilterException(string message, IQueryFilter filter, AggregateException aggregateException) 
            : base(message, aggregateException)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            Details["Filter"] = filter.GetType().FullName;
        }
    }
}