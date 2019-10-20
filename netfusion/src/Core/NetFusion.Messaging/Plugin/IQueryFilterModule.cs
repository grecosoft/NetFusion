using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Messaging.Plugin
{
    /// <summary>
    /// Module interface for managing the filters that are to be invoked
    /// for a dispatched query.
    /// </summary>
    public interface IQueryFilterModule : IPluginModuleService
    {
        /// <summary>
        /// The types of filters registered by the application's bootstrap configuration.
        /// </summary>
        IEnumerable<Type> QueryFilterTypes { get; }
    }
}
