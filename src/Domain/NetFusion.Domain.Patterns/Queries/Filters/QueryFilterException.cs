using NetFusion.Base.Exceptions;
using NetFusion.Common.Extensions.Tasks;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Patterns.Queries.Filters
{
    /// <summary>
    /// Contains information about a query filter that threw an exception when applied.
    /// </summary>
    public class QueryFilterException : NetFusionException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="taskItem">Task result containing the exception.</param>
        public QueryFilterException(TaskListItem<IQueryFilter> taskItem)
        {
            if (taskItem == null) throw new ArgumentNullException(nameof(taskItem));

            var taskException = taskItem.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Filter", taskItem.Invoker.GetType().FullName }
            };
        }
    }
}