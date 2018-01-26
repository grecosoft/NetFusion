using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Domain.Patterns.Queries.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Patterns.Queries.Filters
{
    /// <summary>
    /// Module that manages filters to be invoked when a query is dispatched.
    /// </summary>
    public class QueryFilterModule : PluginModule, IQueryFilterModule
    {
        private QueryDispatchConfig _queryDispatchConfig;

        public override void Initialize()
        {
            _queryDispatchConfig = Context.Plugin.GetConfig<QueryDispatchConfig>();
        }

        // The pre and post filters configured for the application host during bootstrap configuration.
        public IReadOnlyCollection<Type> PreFilterTypes => _queryDispatchConfig.PreQueryFilters;
        public IReadOnlyCollection<Type> PostFilterTypes => _queryDispatchConfig.PostQueryFiters;

        // Registers the pre and post filters within the container so the can 
        // inject needed services.
        public override void RegisterComponents(ContainerBuilder builder)
        {
            RegisterQueryFilters(builder, PreFilterTypes);
            RegisterQueryFilters(builder, PostFilterTypes);
        }

        private void RegisterQueryFilters(ContainerBuilder builder, IEnumerable<Type> filterTypes)
        {
            builder.RegisterTypes(filterTypes.ToArray())
                .AsSelf()
                .InstancePerLifetimeScope();
        }
    }
}
