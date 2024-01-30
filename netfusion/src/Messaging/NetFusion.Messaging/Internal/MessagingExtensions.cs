using System.Reflection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Routing;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    /// Extensions used by the messaging implementation providing methods to 
    /// filter types for message consumers.
    /// </summary>
    internal static class MessagingExtensions
    {
        /// <summary>
        /// Finds all types that are consumers of messages.  This is all types that
        /// implement the IMessageConsumer interface.
        /// </summary>
        /// <param name="types">The types to search.</param>
        /// <returns>Filtered list of types that are message consumers.</returns>
        public static IEnumerable<Type> WhereMessageConsumer(this IEnumerable<Type> types)
        {
            ArgumentNullException.ThrowIfNull(types);

            return types.Where(t => t.IsConcreteTypeDerivedFrom<IMessageConsumer>());
        }

        /// <summary>
        /// For a list of consumer types, returns the methods corresponding to handlers.  
        /// </summary>
        /// <param name="types">Event consumer types to filter.</param>
        /// <returns>List of methods that can handle messages.</returns>
        public static IEnumerable<MethodInfo> SelectMessageHandlers(this IEnumerable<Type> types)
        {
            ArgumentNullException.ThrowIfNull(types);

            return types.SelectMany(ec => ec.GetMethods()
                .Where(IsMessageHandlerMethod));
        }

        private static bool IsMessageHandlerMethod(MethodInfo methodInfo)
        {
            return methodInfo is { IsStatic: false, IsPublic: true }
                   && HasValidParameterTypes(methodInfo);
        }

        private static bool HasValidParameterTypes(MethodInfo methodInfo)
        {
            var paramTypes = methodInfo.GetParameterTypes();
           
            if (paramTypes.Length == 1 && paramTypes[0].CanAssignTo<IMessage>())
            {
                return true;
            }

            return paramTypes.Length == 2 && paramTypes[0].CanAssignTo<IMessage>()
                                          && paramTypes[1].CanAssignTo<CancellationToken>() 
                                          && methodInfo.IsTaskMethod();
        }

        /// <summary>
        /// For a list of methods, returns an object with properties that can be cached and
        /// used to dispatch the method at runtime.
        /// </summary>
        /// <param name="messageHandlers">List of message handler methods.</param>
        /// <returns>List of objects with information used to dispatch the method at runtime.</returns>
        public static IEnumerable<MessageRoute> SelectMessageRoutes(
            this IEnumerable<MethodInfo> messageHandlers)
        {
            ArgumentNullException.ThrowIfNull(messageHandlers);

            return messageHandlers.Select(mi =>
            {
                var messageType = mi.GetParameters().First().ParameterType;
                return mi.ReturnType == typeof(void)
                    ? new ConsumerRoute(messageType, mi)
                    : new ConsumerRoute(messageType, mi.ReturnType, mi);
            });
        }
    }
}