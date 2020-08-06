using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Filters
{
    /// <summary>
    /// Filter executed after the query has been dispatched to the consumer.
    /// </summary>
    public interface IPostQueryFilter : IQueryFilter
    {
        /// <summary>
        /// Invoked after the query has been dispatched to the consumer.
        /// </summary>
        /// <param name="query">The query being dispatched.</param>
        /// <returns>The task that will be completed when execution is completed.</returns>
        Task OnPostExecuteAsync(IQuery query);
    }
}
