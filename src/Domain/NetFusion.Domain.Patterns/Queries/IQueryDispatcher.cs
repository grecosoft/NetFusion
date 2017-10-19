using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Domain.Patterns.Queries
{
    /// <summary>
    /// Injected by an application component to dispatch a query to be handled by a consumer.
    /// </summary>
    public interface IQueryDispatcher
    {
        /// <summary>
        /// Dispatches a query to its corresponding consumer.  When dispatching the
        /// query, it passes through a set of pre/post filters that can alter the
        /// query or consumer returned result.
        /// </summary>
        /// <typeparam name="TResult">The type of the query result.</typeparam>
        /// <param name="query">The query to be dispatched to its corresponding consumer.</param>
        /// <param name="cancellationToken">Optional cancellation token used to cancel the asynchronous task.</param>
        /// <returns>Task containing the result of the consumer.</returns>
        Task<TResult> Dispatch<TResult>(IQuery<TResult> query,
           CancellationToken cancellationToken = default(CancellationToken));
    }
}
