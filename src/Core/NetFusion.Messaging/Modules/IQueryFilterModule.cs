using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// Module interface for the filter module exposing information it manages.
    /// </summary>
    public interface IQueryFilterModule : IPluginModuleService
    {
        /// <summary>
        /// The types of the pre filters registered by the application's bootstrap configuration.
        /// </summary>
        IEnumerable<Type> QueryFilterTypes { get; }
    }
}
