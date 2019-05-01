using System;
using System.Collections.Generic;
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
        {
            if (taskItem == null) throw new ArgumentNullException(nameof(taskItem));

            var taskException = taskItem.Task.Exception;
            var sourceException = taskException?.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Filter", taskItem.Invoker.GetType().FullName }
            };
        }
    }
}