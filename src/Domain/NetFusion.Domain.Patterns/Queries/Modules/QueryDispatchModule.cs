using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Domain.Patterns.Queries.Dispatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NetFusion.Domain.Patterns.Queries.Modules
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
               .SelectDispatchInfo();

            AssureNoDuplicateHandlers(queryHandlers);

            _queryDispatchers = queryHandlers.ToDictionary(qh => qh.QueryType);
        }       

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register the implementation that can be injected into application
            // components to dispatch queries.
            builder.RegisterType<QueryDispatcher>()
                .As<IQueryDispatcher>()
                .InstancePerLifetimeScope();

            RegisterQueryConsumers(builder);
        }

        // Register all query consumers within the container so they can be resolved 
        // to handle executed queries within the current lifetime scope.
        private void RegisterQueryConsumers(ContainerBuilder builder)
        {
            var queryConsumers = _queryDispatchers.Values
               .Select(qd => qd.ConsumerType)
               .Distinct();

            foreach (Type consumerType in queryConsumers)
            {
                builder.RegisterType(consumerType)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .InstancePerLifetimeScope();
            }
        }

        public static void AssureNoDuplicateHandlers(IEnumerable<QueryDispatchInfo> queryHandlers)
        {
            var queryTypes = queryHandlers.WhereDuplicated(qh => qh.QueryType);

            if (queryTypes.Any())
            {
                throw new InvalidOperationException(
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

        public static IEnumerable<QueryDispatchInfo> SelectDispatchInfo(
            this IEnumerable<MethodInfo> queryHandlerMethods)
        {
            foreach (MethodInfo handler in queryHandlerMethods)
            {                
                yield return new QueryDispatchInfo(handler);
            };
        }    
    }
}
