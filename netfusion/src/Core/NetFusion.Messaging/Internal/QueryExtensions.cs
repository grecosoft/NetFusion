using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    /// Extensions used by the messaging implementation providing methods to 
    /// filter types for query consumers.
    /// </summary>
    internal static class QueryExtensions
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
                   && methodInfo.HasAttribute<InProcessHandlerAttribute>()
                   && HasValidParameterTypes(methodInfo);
        }

        private static bool HasValidParameterTypes(MethodInfo methodInfo)
        {
            var paramTypes = methodInfo.GetParameterTypes();

            if (paramTypes.Length == 1 && paramTypes[0].CanAssignTo<IQuery>())
            {
                return true;
            }

            return paramTypes.Length == 2 && paramTypes[0].CanAssignTo<IQuery>() 
                   && paramTypes[1].CanAssignTo<CancellationToken>() && methodInfo.IsAsyncMethod();
        }

        public static IEnumerable<QueryDispatchInfo> SelectQueryDispatchInfo(
            this IEnumerable<MethodInfo> queryHandlerMethods)
        {
            return queryHandlerMethods.Select(handler => new QueryDispatchInfo(handler));
        }    
    }
}