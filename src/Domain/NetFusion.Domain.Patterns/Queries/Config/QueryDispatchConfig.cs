using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Patterns.Queries.Filters;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Patterns.Queries.Config
{
    /// <summary>
    /// Plug-in bootstrap configuration used to specify query dispatching
    /// settings and overrides.
    /// </summary>
    public class QueryDispatchConfig : IContainerConfig
    {
        private List<Type> _preQueryFilterTypes = new List<Type>();
        private List<Type> _postQueryFilterTypes = new List<Type>();

        /// <summary>
        /// The types of the query filters that should be executed before
        /// dispatching the query to the consumer.
        /// </summary>
        public IReadOnlyCollection<Type> PreQueryFilters { get; }

        /// <summary>
        /// The types of the query filters that should be executed after
        /// dispatching the query to the consumer.
        /// </summary>
        public IReadOnlyCollection<Type> PostQueryFiters { get; }

        public QueryDispatchConfig()
        {
            PreQueryFilters = _preQueryFilterTypes.AsReadOnly();
            PostQueryFiters = _postQueryFilterTypes.AsReadOnly();
        }

        /// <summary>
        /// Adds a query filter that should be executed before dispatching 
        /// the query to the consumer.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        public void AddPreQueryFilter<TFilter>() where TFilter : IQueryFilter
        {
            if(_preQueryFilterTypes.Contains(typeof(TFilter)))
            {
                throw new InvalidOperationException(
                    $"The pre-query filter of type: {typeof(TFilter)} has already been added.");
            }
            _preQueryFilterTypes.Add(typeof(TFilter));
        }

        /// <summary>
        /// Adds a query filter that should be executed after dispatching
        /// the query to the consumer.
        /// </summary>
        /// <typeparam name="TFilter"></typeparam>
        public void AddPostQueryFilter<TFilter>() where TFilter : IQueryFilter
        {
            if (_postQueryFilterTypes.Contains(typeof(TFilter)))
            {
                throw new InvalidOperationException(
                    $"The post-query filter of type: {typeof(TFilter)} has already been added.");
            }
            _postQueryFilterTypes.Add(typeof(TFilter));
        }
    }
}
