using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging.Modules
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
        public IReadOnlyCollection<Type> QueryFilterTypes => _queryDispatchConfig.QueryFilters;

        // Registers the pre and post filters within the container so the can 
        // inject needed services.
        public override void RegisterServices(IServiceCollection services)
        {
            foreach (var queryFilterType in QueryFilterTypes)
            {
                services.AddScoped(queryFilterType);
            }
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Query:Filters"] = Context.AllPluginTypes
               .Where(pt => {
                   return pt.IsConcreteTypeDerivedFrom<IQueryFilter>();
               })
               .Select(ft => new
               {
                   FilterType = ft.AssemblyQualifiedName,
                   IsConfigured = _queryDispatchConfig.QueryFilters.Contains(ft),
                   IsPreFilter = ft.IsConcreteTypeDerivedFrom<IPreQueryFilter>(),
                   IsPostFilter = ft.IsConcreteTypeDerivedFrom<IPostQueryFilter>()
               }).ToArray();      
        }
    }
}
