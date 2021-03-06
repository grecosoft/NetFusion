﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin.Modules
{
    /// <summary>
    /// Plug-in module called during the bootstrap process to configure 
    /// the dispatching of queries to consumers.
    /// </summary>
    public class QueryDispatchModule : PluginModule,
        IQueryDispatchModule
    {
        private IDictionary<Type, QueryDispatchInfo> _queryDispatchers; // QueryType => DispatchInfo

        public QueryDispatchConfig DispatchConfig { get; private set; }
        
        // ---------------------- [Plugin Initialization] ----------------------
        
        // Create dictionary used to resolve how a query type is dispatched.  
        public override void Initialize()
        {
            DispatchConfig = Context.Plugin.GetConfig<QueryDispatchConfig>();
            
            var queryHandlers = Context.AllPluginTypes
                .WhereQueryConsumer()
                .SelectQueryHandlers()
                .SelectQueryDispatchInfo()
                .ToArray();

            AssureNoDuplicateHandlers(queryHandlers);

            _queryDispatchers = queryHandlers.ToDictionary(qh => qh.QueryType);
        }       

        // Registers all the query consumers within the service collection.
        public override void ScanForServices(ITypeCatalog catalog)
        {
            catalog.AsSelf(
                t => t.IsConcreteTypeDerivedFrom<IQueryConsumer>(),
                ServiceLifetime.Scoped);
        }

        private static void AssureNoDuplicateHandlers(IEnumerable<QueryDispatchInfo> queryHandlers)
        {
            var queryTypeNames = queryHandlers.WhereDuplicated(qh => qh.QueryType)
                .Select(qt => qt.AssemblyQualifiedName)
                .ToArray();

            if (queryTypeNames.Any())
            {
                throw new QueryDispatchException(
                    $"The following query types have multiple consumers: { string.Join(" | ", queryTypeNames)}." +
                    "A query can only have one consumer.");
            }
        }
        
        // ---------------------- [Plugin Services] ----------------------
        
        public QueryDispatchInfo GetQueryDispatchInfo(Type queryType)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));

            if (_queryDispatchers.TryGetValue(queryType, out QueryDispatchInfo dispatchEntry))
            {
                return dispatchEntry;
            }

            throw new QueryDispatchException(
                $"Dispatch information for the query type: { queryType.AssemblyQualifiedName } is not registered.");
        }
        
        // ---------------------- [Logging] ----------------------

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var messagingDispatchLog = new Dictionary<string, object>();
            moduleLog["QueryConsumers"] = messagingDispatchLog;

            foreach (var (queryType, queryDispatcher) in _queryDispatchers)
            {
                if (queryType.FullName == null) continue;
                
                messagingDispatchLog[queryType.FullName] = new {
                    Consumer = queryDispatcher.ConsumerType.FullName,
                    Method = queryDispatcher.HandlerMethod.Name,
                    queryDispatcher.IsAsync
                };
            }
        }
    }
}
