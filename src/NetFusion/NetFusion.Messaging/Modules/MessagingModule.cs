using Autofac;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// Plug-in module that finds all message related types and stores a lookup used to determine 
    /// what consumer component message handlers should be invoked when a message is published.
    /// </summary>
    public class MessagingModule : PluginModule, 
        IMessagingModule
    {
        // Discovered Properties:
        private IEnumerable<IMessageDispatchRule> DispatchRules { get; set; }

        // IMessagingModule:
        public MessagingConfig MessagingConfig { get; private set; }
        public ILookup<Type, MessageDispatchInfo> AllMessageTypeDispatchers { get; private set; }
        public ILookup<Type, MessageDispatchInfo> InProcessDispatchers { get; private set; }
       
        // All command specific message types.
        private Type[] CommandTypes => InProcessDispatchers
                .Where(ed => ed.Key.IsDerivedFrom<ICommand>())
                .Select(ed => ed.Key).ToArray();
      

        // Stores type meta-data for the message consumers that should be notified when
        // a given message is published. 
        public override void Initialize()
        {
            this.MessagingConfig = this.Context.Plugin.GetConfig<MessagingConfig>();

            IEnumerable<Type> allPluginTypes = this.Context.GetPluginTypesFrom();
            MessageDispatchInfo[] allDispatchers = allPluginTypes
                .WhereEventConsumer()
                .SelectMessageHandlers()
                .SelectDispatchInfo()
                .ToArray();

            SetDispatchRules(allDispatchers);
            SetDispatchPredicateScripts(allDispatchers);
            AssertDispatchRules(allDispatchers);

            this.AllMessageTypeDispatchers = allDispatchers
                .ToLookup(k => k.MessageType);

            this.InProcessDispatchers = allDispatchers
                .Where(h => h.IsInProcessHandler)
                .ToLookup(k => k.MessageType);

            AssertCommandMessages();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register the common messaging service used to publish messages.
            builder.RegisterType<MessagingService>()
                .As<IMessagingService>()
                .InstancePerLifetimeScope();

            // Register all of the message publishers that determine how a given
            // message is delivered.
            builder.RegisterTypes(this.MessagingConfig.PublisherTypes)
                .As<IMessagePublisher>()
                .InstancePerLifetimeScope();
        }

        public override void ScanAllOtherPlugins(TypeRegistration registration)
        {
            registration.PluginTypes.AssignableTo<IMessageConsumer>()
                .As<IMessageConsumer>()
                .AsSelf()
                .InstancePerLifetimeScope();
        }

        private void SetDispatchRules(MessageDispatchInfo[] allDispatchers)
        {
            allDispatchers.ForEach(SetDispatchRule);
        }

        private void SetDispatchRule(MessageDispatchInfo dispatchInfo)
        {
            dispatchInfo.DispatchRules = this.DispatchRules
                .Where(r => dispatchInfo.DispatchRuleTypes.Contains(r.GetType()))
                .ToArray(); 
        }

        private void SetDispatchPredicateScripts(MessageDispatchInfo[] allDispatchers)
        {
            foreach (var dispatcher in allDispatchers)
            {
                var scriptAttrib = dispatcher.MessageHandlerMethod.GetAttribute<ApplyScriptPredicateAttribute>();
                dispatcher.Predicate = scriptAttrib?.ToPredicate();
            }
        }

        // Assert that all command messages have one and only one event
        // dispatch handler. 
        private void AssertCommandMessages()
        {
            var invalidMessageDispatches = this.CommandTypes
                .Select(InProcessDispatchers.WhereHandlerForMessage)
                .Where(di => di.Count() > 1)
                .SelectMany(di => di)
                .Select(di => new
                {
                    di.MessageType,
                    di.ConsumerType,
                    di.MessageHandlerMethod.Name
                }).ToList();

            if (invalidMessageDispatches.Any())
            {
                throw new ContainerException(
                   $"A message that is of type: {typeof(ICommand)} cannot have more than one " +
                   $"consumer message handler.", invalidMessageDispatches);
            }
        }

        // Assert that the dispatch rules are based on a type compatible with the 
        // message.  This applies to dispatch rules that are applied via attributes.
        private void AssertDispatchRules(MessageDispatchInfo[] allDispatchers)
        {
            var invalidEvtHandlers = allDispatchers
                .Where(ed => ed.DispatchRules.Any(
                    dr => !ed.MessageType.IsDerivedFrom(dr.EventType)))
                .Select(ed => new {
                    ed.MessageType,
                    ed.ConsumerType,
                    ed.MessageHandlerMethod.Name
                });
                
            if (invalidEvtHandlers.Any())
            {
                throw new ContainerException(
                    $"The following message consumers have invalid attributes applied " +
                    $"dispatch rules", invalidEvtHandlers);
            }
        }

        public MessageDispatchInfo GetInProcessCommandDispatcher(Type commandType)
        {
            Check.NotNull(commandType, nameof(commandType));
            Check.IsTrue(commandType.IsDerivedFrom<ICommand>(), nameof(commandType), "must be command type");

            IEnumerable<MessageDispatchInfo> dispatchers = this.InProcessDispatchers.WhereHandlerForMessage(commandType);
            if (dispatchers.Empty())
            {
                throw new InvalidOperationException(
                    $"Message dispatcher could not be found for command type: {commandType}");
            }

            if (dispatchers.Count() > 1) {
                throw new InvalidOperationException(
                    $"Command type: {commandType} can't have more than one dispatcher.");
            }

            return dispatchers.First();
        }

        public async Task<object> InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message)
        {
            if (!message.GetType().IsDerivedFrom(dispatcher.MessageType))
            {
                throw new ContainerException(
                    $"The message event type: {message.GetType()} being dispatched does not match or " +
                    $"derived from the dispatch information type of: {dispatcher.MessageType}.");
            }

            using (var scope = AppContainer.Instance.Services.BeginLifetimeScope())
            {
                // Resolve the component and call the message handler.
                var consumer = (IMessageConsumer)scope.Resolve(dispatcher.ConsumerType);
                return await dispatcher.Dispatch(message, consumer);
            }
        }

        // For each discovered message event type, execute the same code that 
        // is used at runtime to determine the consumer methods that handle
        // the message.  Then log this information.
        public override void Log(IDictionary<string, object> moduleLog)
        {
            LogMessagesAndDispatchInfo(moduleLog);
            LogMessagePublishers(moduleLog);
        }

        private void LogMessagesAndDispatchInfo(IDictionary<string, object> moduleLog)
        {
            var messagingDispatchLog = new Dictionary<string, object>();
            moduleLog["Messaging - In Process Dispatchers"] = messagingDispatchLog;

            foreach (var messageTypeDispatcher in this.InProcessDispatchers)
            {
                var messageType = messageTypeDispatcher.Key;
                var messageDispatchers = InProcessDispatchers.WhereHandlerForMessage(messageType);

                messagingDispatchLog[messageType.FullName] = messageDispatchers.Select(
                    ed => new
                    {
                        Consumer = ed.ConsumerType.FullName,
                        Method = ed.MessageHandlerMethod.Name,
                        EventType = ed.MessageType.Name,
                        IsAsync = ed.IsAsync,
                        IncludedDerivedTypes = ed.IncludeDerivedTypes,
                        DispatchRules = ed.DispatchRuleTypes.Select(dr => dr.FullName).ToArray(),
                        RuleApplyType = ed.DispatchRules.Any() ? ed.RuleApplyType.ToString() : null
                    });
            }
        }

        private void LogMessagePublishers(IDictionary<string, object> moduleLog)
        {
            moduleLog["Message Publishers"] = Context.GetPluginTypesFrom()
                .Where(pt => pt.IsDerivedFrom<IMessagePublisher>() && pt.IsClass && !pt.IsAbstract)
                .Select(t => new
                {
                    PublisherType = t.AssemblyQualifiedName,
                    IsConfigured = this.MessagingConfig.PublisherTypes.Contains(t)
                }).ToArray();
        }






        // -- TODO:  this code will be refactored to use the code that has been moved down
        // -- into the MessageDispatchInfo class.

        /// <summary>
        /// Lower level methods used to dispatch a method to a specified  consumer.  The consumer will 
        /// be instantiated and dispatched the message within the context of a new lifetime scope.  This
        /// method should be called when an event needs to be dispatched outside of the current lifetime 
        /// scope (i.e. The one associated with a web request).
        /// </summary>
        /// <param name="message">The message to dispatch.</param>
        /// <param name="dispatchInfo">The message dispatch information.</param>
        public async Task<T> DispatchConsumer<T>(IMessage message, MessageDispatchInfo dispatchInfo)
            where T : class
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(dispatchInfo, nameof(dispatchInfo));

            if (! message.GetType().IsDerivedFrom(dispatchInfo.MessageType))
            {
                throw new ContainerException(
                    $"The message event type: {message.GetType()} being dispatched does not match or " + 
                    $"derived from the dispatch information type of: {dispatchInfo.MessageType}.");
            }
            
            using (var scope = AppContainer.Instance.Services.BeginLifetimeScope())
            {
                // Resolve the component and call the message handler.
                var consumer = scope.Resolve(dispatchInfo.ConsumerType);
                object response = null;

                if (dispatchInfo.IsAsync)
                {
                    var futureResult = (Task)dispatchInfo.Invoker.DynamicInvoke(consumer, message);
                    await futureResult;
                    response = futureResult;
                }
                else
                {
                    response = dispatchInfo.Invoker.DynamicInvoke(consumer, message);
                }

                // The response may be task containing the resulting message 
                // or the message directly if the method was called synchronous. 
                if (response != null)
                {
                    return GetMessageResult<T>(response);
                }
                return null;
            }
        }

        private static T GetMessageResult<T>(object response)
            where T : class
        {
            var responseType = response.GetType();

            if (response is T)
            {
                return response as T;
            }

            if (responseType.IsClosedGenericTypeOf(typeof(Task<>), typeof(T)))
            {
                dynamic messageTask = response;
                return (T)messageTask.Result;
            }

            return null;
        }
    }
}
