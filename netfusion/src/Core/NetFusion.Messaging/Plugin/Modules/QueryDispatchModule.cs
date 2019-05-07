using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Exceptions;

namespace NetFusion.Messaging.Plugin.Modules
{
    /// <summary>
    /// Plug-in module called during the bootstrap process to configure and cache
    /// the dispatching of queries to consumers.
    /// </summary>
    public class QueryDispatchModule : PluginModule,
        IQueryDispatchModule
    {
        private IDictionary<Type, QueryDispatchInfo> _queryDispatchers; // QueryType => DispatchInfo

        // Create dictionary used to resolve how a query type is dispatched.  
        public override void Initialize()
        {
            var queryHandlers = Context.AllPluginTypes
                .WhereQueryConsumer()
                .SelectQueryHandlers()
                .SelectQueryDispatchInfo()
                .ToArray();

            AssureNoDuplicateHandlers(queryHandlers);

            _queryDispatchers = queryHandlers.ToDictionary(qh => qh.QueryType);
        }       

        // Registers all the query consumers within the service collection.
        public override void ScanPlugins(ITypeCatalog catalog)
        {
            catalog.AsSelf(
                t => t.IsConcreteTypeDerivedFrom<IQueryConsumer>(),
                ServiceLifetime.Scoped);
        }

        public static void AssureNoDuplicateHandlers(IEnumerable<QueryDispatchInfo> queryHandlers)
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

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var messagingDispatchLog = new Dictionary<string, object>();
            moduleLog["Query:Consumers"] = messagingDispatchLog;

            foreach (var queryDispatcherRegistration in _queryDispatchers)
            {
                var queryType = queryDispatcherRegistration.Key;
                var queryDispatcher = queryDispatcherRegistration.Value;
                var queryTypeName = queryType.FullName;

                if (queryTypeName == null) continue;
                
                messagingDispatchLog[queryTypeName] = new {
                    Consumer = queryDispatcher.ConsumerType.FullName,
                    Method = queryDispatcher.HandlerMethod.Name,
                    queryDispatcher.IsAsync
                };
            }
        }
    }
}
