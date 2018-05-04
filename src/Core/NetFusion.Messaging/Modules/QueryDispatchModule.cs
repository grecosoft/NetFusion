using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// Plug-in module called during the bootstrap process to configure
    /// the dispatching of queries to consumers.
    /// </summary>
    public class QueryDispatchModule : PluginModule,
        IQueryDispatchModule
    {
        private IDictionary<Type, QueryDispatchInfo> _queryDispatchers; // QueryType => DispachInfo

        // Create dictionary used to resolve how a query type is dispatched.  
        public override void Initialize()
        {
            var queryHandlers = Context.AllPluginTypes
               .WhereQueryConsumer()
               .SelectQueryHandlers()
               .SelectQueryDispatchInfo();

            AssureNoDuplicateHandlers(queryHandlers);

            _queryDispatchers = queryHandlers.ToDictionary(qh => qh.QueryType);
        }       

        public override void RegisterServices(IServiceCollection services)
        {
            // Register the dispatcher used specifically for queries.
            services.AddScoped<QueryDispatcher>();

            RegisterQueryConsumers(services);
        }

        // Register all query consumers within the container so they can be resolved 
        // to handle executed queries within the current lifetime scope.
        private void RegisterQueryConsumers(IServiceCollection services)
        {
            var queryConsumers = _queryDispatchers.Values
               .Select(qd => qd.ConsumerType)
               .Distinct()
               .ToArray();

            foreach (Type queryConsumer in queryConsumers)
            {
                services.AddScoped(queryConsumer);
            }
        }

        public static void AssureNoDuplicateHandlers(IEnumerable<QueryDispatchInfo> queryHandlers)
        {
            var queryTypes = queryHandlers.WhereDuplicated(qh => qh.QueryType);

            if (queryTypes.Any())
            {
                throw new QueryDispatchException(
                    $"The following query types have multiple consumers: { String.Join(" | ", queryTypes)}." +
                    $"A query can only have one consumer.");
            }
        }

        public QueryDispatchInfo GetQueryDispatchInfo(Type queryType)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType)); ;

            if (_queryDispatchers.TryGetValue(queryType, out QueryDispatchInfo dispatchEntry))
            {
                return dispatchEntry;
            }

            throw new QueryDispatchException(
                $"Dispatch information for the query type: { queryType.FullName } is not registered.");
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            var messagingDispatchLog = new Dictionary<string, object>();
            moduleLog["Query:Consumers"] = messagingDispatchLog;

            foreach (var queryDispatcherRegistration in _queryDispatchers)
            {
                var queryType = queryDispatcherRegistration.Key;
                var queryDispatcher = queryDispatcherRegistration.Value;

                messagingDispatchLog[queryType.FullName] = new {
                    Consumer = queryDispatcher.ConsumerType.FullName,
                    Method = queryDispatcher.HandlerMethod.Name,
                    IsAsync = queryDispatcher.IsAsync
                };
            }
        }
    }

    static internal class QueryExtensions
    {
        // Find all plug-in types that know how to process a query.
        public static IEnumerable<Type> WhereQueryConsumer(this IEnumerable<Type> pluginTypes)
        {
            return pluginTypes.Where(pt => pt.IsConcreteTypeDerivedFrom<IQueryConsumer>());
        }

        // Returns all methods that are query handlers.
        public static IEnumerable<MethodInfo> SelectQueryHandlers(this IEnumerable<Type> queryHandlerTypes)
        {
            return queryHandlerTypes.SelectMany(ec => ec.GetMethods()
                 .Where(IsQueryHandlerMethod));
        }

        private static bool IsQueryHandlerMethod(MethodInfo methodInfo)
        {
            return !methodInfo.IsStatic
                && methodInfo.IsPublic
                && HasValidParameterTypes(methodInfo);
        }

        private static bool HasValidParameterTypes(MethodInfo methodInfo)
        {
            var paramTypes = methodInfo.GetParameterTypes();

            if (paramTypes.Length == 1 && paramTypes[0].CanAssignTo<IQuery>())
            {
                return true;
            }

            if (paramTypes.Length == 2 && paramTypes[0].CanAssignTo<IQuery>()
                && (paramTypes[1].CanAssignTo<CancellationToken>() && methodInfo.IsAsyncMethod()))
            {
                return true;
            }

            return false;
        }

        public static IEnumerable<QueryDispatchInfo> SelectQueryDispatchInfo(
            this IEnumerable<MethodInfo> queryHandlerMethods)
        {
            foreach (MethodInfo handler in queryHandlerMethods)
            {                
                yield return new QueryDispatchInfo(handler);
            };
        }    
    }
}
