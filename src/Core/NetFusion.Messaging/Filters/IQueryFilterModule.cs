using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging.Filters
{
    /// <summary>
    /// Module interface for the filter module exposing information it manages.
    /// </summary>
    public interface IQueryFilterModule : IPluginModuleService
    {
        /// <summary>
        /// The types of the pre filters registered by the application's bootstrap configuration.
        /// </summary>
        IReadOnlyCollection<Type> PreFilterTypes { get; }

        /// <summary>
        /// The types of the post filters registered by the application's bootstrap configuration.
        /// </summary>
        IReadOnlyCollection<Type> PostFilterTypes { get; }
    }
}
