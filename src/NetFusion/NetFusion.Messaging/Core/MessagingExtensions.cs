using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Messaging.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

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
        public static IEnumerable<Type> WhereEventConsumer(this IEnumerable<Type> types)
        {
            Check.NotNull(types, nameof(types));
            return types.Where(t => t.IsDerivedFrom<IMessageConsumer>() && !t.IsInterface);
        }

        /// <summary>
        /// For a list of consumer types, returns the methods corresponding to handlers.  
        /// </summary>
        /// <param name="types">Event consumer types to filter.</param>
        /// <returns>List of methods that can handle messages.</returns>
        public static IEnumerable<MethodInfo> SelectMessageHandlers(this IEnumerable<Type> types)
        {
            Check.NotNull(types, nameof(types));

            return types.SelectMany(ec => ec.GetMethods()
                .Where(IsMessageHandlerMethod));
        }

        private static bool IsMessageHandlerMethod(MethodInfo methodInfo)
        {
            var isCorrectMethodType = !methodInfo.IsStatic
                && methodInfo.IsPublic
                && methodInfo.GetParameters().Length == 1;

            if (!isCorrectMethodType) return false;
            
            var methodParam = methodInfo.GetParameters().First();
            return methodParam.ParameterType.IsDerivedFrom<IMessage>();
        }

        /// <summary>
        /// For a list of methods, returns an object with properties that can be used to 
        /// dispatch the method at runtime.
        /// </summary>
        /// <param name="messageHandlers">List of message handler methods.</param>
        /// <returns>List of objects with information used to dispatch the method at runtime.</returns>
        public static IEnumerable<MessageDispatchInfo> SelectDispatchInfo(this IEnumerable<MethodInfo> messageHandlers)
        {
            Check.NotNull(messageHandlers, nameof(messageHandlers));

            return messageHandlers.Select(mi => new MessageDispatchInfo
            {
                MessageType = mi.GetParameters().First().ParameterType,
                ConsumerType = mi.DeclaringType,
                Invoker = GetMethodDispatch(mi),
                IncludeDerivedTypes = IncludeDerivedTypes(mi.GetParameters().First()),
                DispatchRuleTypes = GetOptionalRuleTypes(mi),
                RuleApplyType = GetOptionalRuleApplyType(mi),
                MessageHandlerMethod = mi,
                IsInProcessHandler = IsInProcessHandler(mi),
                IsAsync = IsAsyncDispatch(mi)
            });
        }

        // Creates a delegate representing a reflected MethodInfo for the consumer's
        // message handler.  This make the call almost as fast as a direct call.
        private static MulticastDelegate GetMethodDispatch(MethodInfo methodInfo)
        {
            var paramTypes = new List<Type>
            {
                methodInfo.DeclaringType,                           // Consumer Type
                methodInfo.GetParameters().First().ParameterType,   // Message Type
                methodInfo.ReturnType                               // Optional return type
            };

            var dispatchType = Expression.GetDelegateType(paramTypes.ToArray());
            return (MulticastDelegate)methodInfo.CreateDelegate(dispatchType);
        }

        private static bool IsAsyncDispatch(MethodInfo methodInfo)
        {
            return methodInfo.ReturnType != null && methodInfo.ReturnType.IsDerivedFrom<Task>();
        }

        private static bool IncludeDerivedTypes(ParameterInfo parameterInfo)
        {
            return parameterInfo.GetCustomAttribute<IncludeDerivedMessagesAttribute>() != null;
        }

        private static Type[] GetOptionalRuleTypes(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<ApplyDispatchRuleAttribute>()?.RuleTypes ?? new Type[] { };
        }

        private static bool IsInProcessHandler(MethodInfo methodInfo)
        {
            return methodInfo.HasAttribute<InProcessHandlerAttribute>();
        }

        private static RuleApplyTypes GetOptionalRuleApplyType(MethodInfo methodInfo)
        {
            return methodInfo.GetCustomAttribute<ApplyDispatchRuleAttribute>()?.RuleApplyType ?? RuleApplyTypes.All;
        }

        /// <summary>
        /// Given a lookup of message dispatch information keyed by message type,
        /// finds the event handlers that should be called to handle the event.
        /// </summary>
        /// <param name="messageTypeHandlers">Lookup of message handlers.</param>
        /// <param name="messageType">The message type being published.</param>
        /// <returns>List dispatch information for the handlers that should be
        /// invoked for the message.</returns>
        public static IEnumerable<MessageDispatchInfo> WhereHandlerForMessage(
            this ILookup<Type, MessageDispatchInfo> messageTypeHandlers, 
            Type messageType)
        {
            Check.NotNull(messageTypeHandlers, nameof(messageTypeHandlers));
            Check.NotNull(messageType, nameof(messageType));

            // A handler method defined for the message type will be invoked.
            // Message handlers for base message types will be included if specified. 
            return messageTypeHandlers
                .Where(di => di.Key.IsAssignableFrom(messageType))
                .SelectMany(di => di)
                .Where(di =>  
                    (di.IncludeDerivedTypes || di.MessageType == messageType));
        }
    }
}
