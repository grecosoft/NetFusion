using System;
using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions.Tasks;
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
    }
    
    /// <summary>
    /// Contains information about a specific type of query filter that threw
    /// an exception when applied.
    /// </summary>
    public class QueryFilterException<T> : QueryFilterException
        where T : class, IQueryFilter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="taskItem">Task result containing the exception.</param>
        public QueryFilterException(TaskListItem<T> taskItem) 
            : base("Query Filter Exception", GetSourceException(taskItem))
        {
            if (taskItem == null) throw new ArgumentNullException(nameof(taskItem));

            Details["Filter"] = taskItem.Invoker.GetType().FullName;
        }
        
        private static Exception GetSourceException(TaskListItem<T> taskItem)
        {
            // Get the aggregate inner exception.
            var taskException = taskItem.Task.Exception;
            return taskException?.InnerException;
        }
    }
}