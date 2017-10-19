using NetFusion.Base.Plugins;
using System.Threading.Tasks;

namespace NetFusion.Domain.Patterns.Queries.Filters
{
    /// <summary>
    /// Interface implemented by classes that are invoked when a query is dispatched.
    /// </summary>
    public interface IQueryFilter : IKnownPluginType
    {
        /// <summary>
        /// Invoked with the query being dispatched.
        /// </summary>
        /// <param name="query">The dispatched query.</param>
        /// <returns>Task.</returns>
        Task OnExecute(IQuery query);
    }
}
