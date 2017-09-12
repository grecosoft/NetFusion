using Autofac;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Domain.Messaging;
using NetFusion.Domain.Messaging.Rules;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
      
        // Stores type meta-data for the message consumers that should be notified when
        // a given message is published. 
        public override void Initialize()
        {
            MessagingConfig = Context.Plugin.GetConfig<MessagingConfig>();

            MessageDispatchInfo[] allDispatchers = this.Context.AllPluginTypes
                .WhereEventConsumer()
                .SelectMessageHandlers()
                .SelectDispatchInfo()
                .ToArray();

            SetDispatchRules(allDispatchers);
            SetDispatchPredicateScripts(allDispatchers);
            AssertDispatchRules(allDispatchers);

            AllMessageTypeDispatchers = allDispatchers
                .ToLookup(k => k.MessageType);

            InProcessDispatchers = allDispatchers
                .Where(h => h.IsInProcessHandler)
                .ToLookup(k => k.MessageType);

            LogInvalidConsumers();
        }

        public override void RegisterDefaultComponents(ContainerBuilder builder)
        {
            // Register the common messaging service used to publish messages.
            builder.RegisterType<MessagingService>()
                .As<IMessagingService>()
                .InstancePerLifetimeScope();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register all of the message publishers that determine how a given
            // message is delivered.  This is how the message dispatch pipeline
            // is extended.
            builder.RegisterTypes(MessagingConfig.PublisherTypes)
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

        // Lookup the dispatch rules specified on the message consumer handler and
        // store a reference to the associated rule object.
        private void SetDispatchRule(MessageDispatchInfo dispatchInfo)
        {
            dispatchInfo.DispatchRules = DispatchRules
                .Where(r => dispatchInfo.DispatchRuleTypes.Contains(r.GetType()))
                .ToArray(); 
        }

        // Check all message consumer handlers having the ApplyScriptPredicate attribute and
        // store a reference to a ScriptPredicate instance indicating the script to be executed
        // at runtime to determine if the message handler should be invoked.
        private void SetDispatchPredicateScripts(MessageDispatchInfo[] allDispatchers)
        {
            foreach (var dispatcher in allDispatchers)
            {
                var scriptAttrib = dispatcher.MessageHandlerMethod.GetAttribute<ApplyScriptPredicateAttribute>();
                dispatcher.Predicate = scriptAttrib?.ToPredicate();
            }
        }

        // Assert that the dispatch rules are based on a type compatible with the 
        // message.  This applies to dispatch rules that are applied via attributes.
        private void AssertDispatchRules(MessageDispatchInfo[] allDispatchers)
        {
            var invalidEvtHandlers = allDispatchers
                .Where(d => d.DispatchRules.Any(
                    dr => !d.MessageType.CanAssignTo(dr.MessageType)))
                .Select(d => new {
                    d.MessageType,
                    d.ConsumerType,
                    d.MessageHandlerMethod.Name
                });
                
            if (invalidEvtHandlers.Any())
            {
                throw new ContainerException(
                    "The following message consumers have invalid rule attributes applied.  " +
                    "The handler message type and the rule message type must be assignable to each other.", 
                    invalidEvtHandlers);
            }
        }

        public MessageDispatchInfo GetInProcessCommandDispatcher(Type commandType)
        {
            Check.NotNull(commandType, nameof(commandType));
            Check.IsTrue(commandType.IsDerivedFrom<ICommand>(), nameof(commandType), "must be command type");

            IEnumerable<MessageDispatchInfo> dispatchers = InProcessDispatchers.WhereHandlerForMessage(commandType);
            if (dispatchers.Empty())
            {
                throw new InvalidOperationException(
                    $"Message dispatcher could not be found for command type: {commandType}");
            }

            return dispatchers.First();
        }

        public async Task<object> InvokeDispatcherAsync(MessageDispatchInfo dispatcher, IMessage message, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(dispatcher, nameof(dispatcher));
            Check.NotNull(message, nameof(message));

            if (!message.GetType().CanAssignTo(dispatcher.MessageType))
            {
                throw new ContainerException(
                    $"The message event type: {message.GetType()} being dispatched does not match or " +
                    $"derived from the dispatch information type of: {dispatcher.MessageType}.");
            }

            // Invoke the message consumers in a new lifetime scope.  This is for the case where a message
            // is received outside of the normal lifetime scope such as the one associated with the current
            // web request.
            using (var scope = AppContainer.Instance.Services.BeginLifetimeScope())
            {
                try
                {
                    // Resolve the component and call the message handler.
                    var consumer = (IMessageConsumer)scope.Resolve(dispatcher.ConsumerType);
                    return await dispatcher.Dispatch(message, consumer, cancellationToken);
                }
                catch (Exception ex)
                {
                    Context.Logger.LogError(MessagingLogEvents.MESSAGING_EXCEPTION, "Message Dispatch Error Details.", ex);
                    throw;
                }
            }
        }

        private void LogInvalidConsumers()
        {
            string[] invalidConsumerTypes = Context.AllPluginTypes
               .SelectMessageHandlers()
               .Where(h => h.HasAttribute<InProcessHandlerAttribute>() && !h.DeclaringType.IsDerivedFrom<IMessageConsumer>())
               .Select(h => h.DeclaringType.AssemblyQualifiedName)
               .Distinct()
               .ToArray();

            if (invalidConsumerTypes.Any())
            {
                Context.Logger.LogWarning(
                    MessagingLogEvents.MESSAGING_CONFIGURATION,
                    $"The following classes have in-process event handler methods but do not implement: {typeof(IMessageConsumer)}.",
                    invalidConsumerTypes);
            }
        }

        // For each discovered message event type, execute the same code that is used at runtime to determine
        // the consumer methods that handle the message.  Then log this information.
        public override void Log(IDictionary<string, object> moduleLog)
        {
            LogMessagesAndDispatchInfo(moduleLog);
            LogMessagePublishers(moduleLog);
        }

        private void LogMessagesAndDispatchInfo(IDictionary<string, object> moduleLog)
        {
            var messagingDispatchLog = new Dictionary<string, object>();
            moduleLog["Messaging - In Process Dispatchers"] = messagingDispatchLog;

            foreach (var messageTypeDispatcher in InProcessDispatchers)
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
            moduleLog["Message Publishers"] = Context.AllPluginTypes
                .Where(pt => {
                    return pt.IsConcreteTypeDerivedFrom<IMessagePublisher>();
                })
                .Select(t => new
                {
                    PublisherType = t.AssemblyQualifiedName,
                    IsConfigured = MessagingConfig.PublisherTypes.Contains(t)
                }).ToArray();
        }
    }
}
