using System;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin
{
    /// <summary>
    /// Plug-in interface exposing information managed by the module.
    /// </summary>
    public interface IQueryDispatchModule : IPluginModuleService
    {
        /// <summary>
        /// The associated query configuration.
        /// </summary>
        public QueryDispatchConfig DispatchConfig { get; }
        
        /// <summary>
        /// Returns information of how a given query should be dispatched.
        /// </summary>
        /// <param name="queryType">The type of the query to be dispatched.</param>
        /// <returns>Dispatch information about the query and its consumer.</returns>
        QueryDispatchInfo GetQueryDispatchInfo(Type queryType);
    }
}
