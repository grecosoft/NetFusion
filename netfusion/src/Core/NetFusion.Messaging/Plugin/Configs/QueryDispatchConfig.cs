using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Filters;

namespace NetFusion.Messaging.Plugin.Configs
{
    /// <summary>
    /// Plug-in bootstrap configuration used to specify query dispatching
    /// settings and overrides.
    /// </summary>
    public class QueryDispatchConfig : IPluginConfig
    {
        private readonly List<Type> _queryFilterTypes = new List<Type>();

        /// <summary>
        /// The query filter types that should be executed when dispatching 
        /// the query to the consumer.
        /// </summary>
        public IReadOnlyCollection<Type> QueryFilters { get; }

        public QueryDispatchConfig()
        {
            QueryFilters = _queryFilterTypes.AsReadOnly();
        }

        /// <summary>
        /// Adds a query filter that should be executed when dispatching 
        /// the query to the consumer.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        public void AddFilter<TFilter>()
            where TFilter : IQueryFilter
        {
            if(_queryFilterTypes.Contains(typeof(TFilter)))
            {
                throw new InvalidOperationException(
                    $"The filter of type: {typeof(TFilter)} has already been added.");
            }
            _queryFilterTypes.Add(typeof(TFilter));
        }
    }
}
