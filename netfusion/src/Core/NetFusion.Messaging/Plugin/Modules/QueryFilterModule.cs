using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin.Modules
{
    /// <summary>
    /// Module that manages filters to be invoked when a query is dispatched.
    /// </summary>
    public class QueryFilterModule : PluginModule
    {
        private QueryDispatchConfig _queryDispatchConfig;

        public override void Initialize()
        {
            _queryDispatchConfig = Context.Plugin.GetConfig<QueryDispatchConfig>();
        }
        
        // ---------------------- [Plugin Initialization] ----------------------

        // Registers the pre and post filters within the container so they can 
        // inject needed services.
        public override void RegisterServices(IServiceCollection services)
        {
            foreach (var queryFilterType in _queryDispatchConfig.QueryFilters)
            {
                services.AddScoped(typeof(IQueryFilter), queryFilterType);
            }
        }
        
        // ---------------------- [Logging] ----------------------

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["QueryFilters"] = Context.AllPluginTypes
               .Where(pt => pt.IsConcreteTypeDerivedFrom<IQueryFilter>())
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
