using NetFusion.Base.Exceptions;
using NetFusion.Common;
using NetFusion.Common.Extensions.Tasks;
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
        /// <param name="futureResult">The future result containing the exception.</param>
        public QueryFilterException(TaskListItem<IQueryFilter> futureResult)
        {
            Check.NotNull(futureResult, nameof(futureResult));

            var taskException = futureResult.Task.Exception;
            var sourceException = taskException.InnerException;

            Details = new Dictionary<string, object>
            {
                { "Message", sourceException?.Message },
                { "Filter", futureResult.Invoker.GetType().FullName }
            };
        }
    }
}