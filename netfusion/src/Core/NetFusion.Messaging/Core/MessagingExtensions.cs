using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Rules;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace NetFusion.Messaging.Core
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
            if (types == null) throw new ArgumentNullException(nameof(types));

            return types.Where(t => t.IsConcreteTypeDerivedFrom<IMessageConsumer>());
        }

        /// <summary>
        /// For a list of consumer types, returns the methods corresponding to handlers.  
        /// </summary>
        /// <param name="types">Event consumer types to filter.</param>
        /// <returns>List of methods that can handle messages.</returns>
        public static IEnumerable<MethodInfo> SelectMessageHandlers(this IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types));

            return types.SelectMany(ec => ec.GetMethods()
                .Where(IsMessageHandlerMethod));
        }

        private static bool IsMessageHandlerMethod(MethodInfo methodInfo)
        {
            return !methodInfo.IsStatic
                && methodInfo.IsPublic
                && HasValidParameterTypes(methodInfo);
        }

        private static bool HasValidParameterTypes(MethodInfo methodInfo)
        {
            var paramTypes = methodInfo.GetParameterTypes();
           
            if (paramTypes.Length == 1 && paramTypes[0].CanAssignTo<IMessage>())
            {
                return true;
            }

            if (paramTypes.Length == 2 && paramTypes[0].CanAssignTo<IMessage>()
                && (paramTypes[1].CanAssignTo<CancellationToken>() && methodInfo.IsAsyncMethod()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// For a list of methods, returns an object with properties that can be cached and
        /// used to dispatch the method at runtime.
        /// </summary>
        /// <param name="messageHandlers">List of message handler methods.</param>
        /// <returns>List of objects with information used to dispatch the method at runtime.</returns>
        public static IEnumerable<MessageDispatchInfo> SelectMessageDispatchInfo(this IEnumerable<MethodInfo> messageHandlers)
        {
            if (messageHandlers == null) throw new ArgumentNullException(nameof(messageHandlers));

            return messageHandlers.Select(mi => new MessageDispatchInfo
            {
                MessageType = mi.GetParameters().First().ParameterType,
                ConsumerType = mi.DeclaringType,
                Invoker = GetMethodDelegate(mi),
                IncludeDerivedTypes = IncludeDerivedTypes(mi.GetParameters().First()),
                DispatchRuleTypes = GetOptionalRuleTypes(mi),
                RuleApplyType = GetOptionalRuleApplyType(mi),
                MessageHandlerMethod = mi,
                IsInProcessHandler = IsInProcessHandler(mi),
                IsAsync = mi.IsAsyncMethod(),
                IsAsyncWithResult = mi.IsAsyncMethodWithResult(),
                IsCancellable = mi.IsCancellableMethod()
            });
        }

        // Creates a delegate representing a reflected MethodInfo for the consumer's
        // message handler.  
        private static MulticastDelegate GetMethodDelegate(MethodInfo methodInfo)
        {
            // Required Handler Parameters:
            var paramTypes = new List<Type>
            {
                methodInfo.DeclaringType,                           // Consumer Type
                methodInfo.GetParameters().First().ParameterType,   // Message Type
            };

            // Optional Handler Parameters:
            if (methodInfo.IsCancellableMethod())
            {
                paramTypes.Add(typeof(CancellationToken));
            }

            paramTypes.Add(methodInfo.ReturnType); // Optional return type

            var dispatchType = Expression.GetDelegateType(paramTypes.ToArray());
            return (MulticastDelegate)methodInfo.CreateDelegate(dispatchType);
        }

        private static bool IsInProcessHandler(MethodInfo methodInfo)
        {
            return methodInfo.HasAttribute<InProcessHandlerAttribute>();
        }

        private static bool IncludeDerivedTypes(ParameterInfo parameterInfo)
        {
            return parameterInfo.HasAttribute<IncludeDerivedMessagesAttribute>();
        }

        private static Type[] GetOptionalRuleTypes(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<ApplyDispatchRuleAttribute>()?.RuleTypes ?? new Type[] { };
        }

        private static RuleApplyTypes GetOptionalRuleApplyType(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<ApplyDispatchRuleAttribute>()?.RuleApplyType ?? RuleApplyTypes.All;
        }

        /// <summary>
        /// Given a lookup of message dispatch information keyed by message type, finds the message 
        /// dispatchers that should be called to handle the message.
        /// </summary>
        /// <param name="messageDispatchers">Lookup of message dispatchers.</param>
        /// <param name="messageType">The message type being published.</param>
        /// <returns>List dispatchers for the handlers that should be invoked for the message.</returns>
        public static IEnumerable<MessageDispatchInfo> WhereHandlerForMessage(
            this ILookup<Type, MessageDispatchInfo> messageDispatchers, 
            Type messageType)
        {
            if (messageDispatchers == null) throw new ArgumentNullException(nameof(messageDispatchers));
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
     
            // A handler method defined for the message type will be invoked.
            // Message handlers for base message types will be included if specified. 
            return messageDispatchers
                .Where(di => di.Key.IsAssignableFrom(messageType))
                .SelectMany(di => di)
                .Where(di => di.IncludeDerivedTypes || di.MessageType == messageType);
        }
    }
}
