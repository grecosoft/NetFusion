using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Patterns.Queries.Dispatch;
using System;

namespace NetFusion.Domain.Patterns.Queries.Modules
{
    /// <summary>
    /// Plug-in interface exposing information managed by the module.
    /// </summary>
    public interface IQueryDispatchModule : IPluginModuleService
    {
        /// <summary>
        /// Returns information of how a given query should be dispatched.
        /// </summary>
        /// <param name="queryType">The type of the query to be dispatched.</param>
        /// <returns>Dispatch information about the query and its consumer.</returns>
        QueryDispatchInfo GetQueryDispatchInfo(Type queryType);
    }
}
