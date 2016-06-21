using Autofac;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
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
        // Imported Instances:
        public IEnumerable<IMessageDispatchRule> DispatchRules { get; private set; }

        // IMessagingModule:
        public MessagingConfig MessagingConfig { get; private set; }
        public ILookup<Type, MessageDispatchInfo> MessageTypeDispatchers { get; private set; }

        // All command specific message types.
        private Type[] CommandTypes => MessageTypeDispatchers
                .Where(ed => ed.Key.IsDerivedFrom<ICommand>())
                .Select(ed => ed.Key).ToArray();
       
        // All message dispatchers.
        private MessageDispatchInfo[] AllMessageDispatchers => MessageTypeDispatchers
            .SelectMany(ed => ed).ToArray();

        // Stores type meta-data for the message consumers that should be notified when
        // a given message is published. 
        public override void Initialize()
        {
            this.MessagingConfig = this.Context.Plugin.GetConfig<MessagingConfig>();

            var allPluginTypes = this.Context.GetPluginTypesFrom();

            this.MessageTypeDispatchers = allPluginTypes
                .WhereEventConsumer()
                .SelectMessageHandlers(this.MessagingConfig.ConsumerMethodPrefix)
                .MarkedWith<InProcessHandlerAttribute>()
                .SelectDispatchInfo()
                .ToLookup(k => k.MessageType);

            SetDispatchRules(this.MessageTypeDispatchers);
            AssertDispatchRules();
            AssertCommandMessages();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<MessagingService>()
                .As<IMessagingService>()
                .InstancePerLifetimeScope();

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

        private void SetDispatchRules(ILookup<Type, MessageDispatchInfo> messageTypeDispatchers)
        {
            this.AllMessageDispatchers.ForEach(SetDispatchRule);
        }

        private void SetDispatchRule(MessageDispatchInfo dispatchInfo)
        {
            dispatchInfo.DispatchRules = this.DispatchRules
                .Where(r => dispatchInfo.DispatchRuleTypes.Contains(r.GetType()))
                .ToArray(); 
        }

        // Assert that all command messages have one and only one event
        // dispatch handler. 
        private void AssertCommandMessages()
        {
            var invalidMessageDispatches = this.CommandTypes
                .Select(MessageTypeDispatchers.WhereHandlerForMessage)
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
                   $"a message that is of type: {typeof(ICommand)} cannot have more than one " +
                   $"consumer message handler.", invalidMessageDispatches);
            }
        }

        // Assert that the dispatch rules are based on a type compatible with the 
        // message.  This applies to dispatch rules that are applied via attributes.
        private void AssertDispatchRules()
        {
            var invalidEvtHanlders = this.AllMessageDispatchers
                .Where(ed => ed.DispatchRules.Any(
                    dr => !ed.MessageType.IsDerivedFrom(dr.EventType)))
                .Select(ed => new {
                    ed.MessageType,
                    ed.ConsumerType,
                    ed.MessageHandlerMethod.Name
                });
                
            if (invalidEvtHanlders.Any())
            {
                var messages = invalidEvtHanlders.Select(h => h.ToJson());
                throw new ContainerException(
                    $"the following message consumers have invalid attributes applied " +
                    $"dispatch rules: {String.Join(", ", messages)}");
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

            foreach (var messageTypeDispatcher in this.MessageTypeDispatchers)
            {
                var messageType = messageTypeDispatcher.Key;
                var messageDispatchers = MessageTypeDispatchers.WhereHandlerForMessage(messageType);

                messagingDispatchLog[messageType.FullName] = messageDispatchers.Select(
                    ed => new
                    {
                        Consumer = ed.ConsumerType.FullName,
                        Method = ed.MessageHandlerMethod.Name,
                        EventType = ed.MessageType.Name,
                        IsAsync = ed.IsAsync,
                        IncludedDerivedTypes = ed.IncludeDerivedTypes,
                        DispatchRules = ed.DispatchRuleTypes.Select(dr => dr.FullName).ToArray(),
                        RuleApplyType = ed.RuleApplyType.ToString()
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
               
        

        /// <summary>
        /// Lower level methods used to dispatch a method to a specified  consumer.  The consumer will 
        /// be instantiated and dispatched the message within the context of a new lifetime scope.  This
        /// method should be called when an event needs to be dispatched outside of the current lifetime 
        /// scope (i.e. The one associated with a web request).
        /// </summary>
        /// <param name="message">The message to dispatch.</param>
        /// <param name="dispatchInfo">The message dispatch information.</param>
        public static async Task<IMessage> DispatchConsumer(IMessage message, MessageDispatchInfo dispatchInfo)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(dispatchInfo, nameof(dispatchInfo));

            if (!message.GetType().IsDerivedFrom(dispatchInfo.MessageType))
            {
                throw new ContainerException(
                    $"the message event type: {message.GetType()} being dispatched does not match or " + 
                    $"derived from the dispatch information type of: {dispatchInfo.MessageType}");
            }

            if (!dispatchInfo.IsMatch(message)) return null;
            
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
                    return GetMessageResult(response);
                }
                return null;
            }
        }

        private static IMessage GetMessageResult(object response)
        {
            var responseType = response.GetType();

            if (response is IMessage)
            {
                return response as IMessage;
            }

            if (responseType.IsClosedGenericTypeOf(typeof(Task<>), typeof(IMessage)))
            {
                dynamic messageTask = response;
                return (IMessage)messageTask.Result;
            }

            return null;
        }
    }
}
