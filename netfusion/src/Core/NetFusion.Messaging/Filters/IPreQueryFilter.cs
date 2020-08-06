using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Filters
{
    /// <summary>
    /// Filter executed before the query is dispatched to the consumer.
    /// </summary>
    public interface IPreQueryFilter : IQueryFilter
    {
        /// <summary>
        /// Invoked before the query is dispatched to the consumer.
        /// </summary>
        /// <param name="query">The query being dispatched.</param>
        /// <returns>The task that will be completed when execution is completed.</returns>
        Task OnPreExecuteAsync(IQuery query);
    }
}
