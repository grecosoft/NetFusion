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
        /// <param name="taskResult">Task result containing the exception.</param>
        public QueryFilterException(TaskListItem<IQueryFilter> taskResult)
        {
            if (taskResult == null) throw new ArgumentNullException(nameof(taskResult));

            var taskException = taskResult.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Filter", taskResult.Invoker.GetType().FullName }
            };
        }
    }
}