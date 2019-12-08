using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.Plugin.Modules
{
    /// <summary>
    /// Plug-in module that finds and caches all message related types and stores a lookup used
    /// to determine what consumer component message handlers should be invoked when a message
    /// is published.  A message can be either a command or a domain-event.
    /// </summary>
    public class MessageDispatchModule : PluginModule, 
        IMessageDispatchModule
    {
        // Discovered Properties:
        private IEnumerable<IMessageDispatchRule> DispatchRules { get; set; }

        // IMessagingModule:
        public MessageDispatchConfig DispatchConfig { get; private set; }
        public ILookup<Type, MessageDispatchInfo> AllMessageTypeDispatchers { get; private set; } // MessageType => Dispatcher(s)
        public ILookup<Type, MessageDispatchInfo> InProcessDispatchers { get; private set; } //MessageType => Dispatcher(s)

        //------------------------------------------------------
        //--Plugin Initialization
        //------------------------------------------------------
        
        // Stores type meta-data for the message consumers that
        // should be notified when a given message is published. 
        public override void Initialize()
        {
            DispatchConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();

            MessageDispatchInfo[] allDispatchers = Context.AllPluginTypes
                .WhereMessageConsumer()
                .SelectMessageHandlers()
                .SelectMessageDispatchInfo()
                .ToArray();

            // Associate optional rules with the dispatchers that 
            // determine if handler applies to a published message.
            SetDispatchRules(allDispatchers);
            SetDispatchPredicateScripts(allDispatchers);
            
            AssertDispatchRules(allDispatchers);

            AllMessageTypeDispatchers = allDispatchers
                .ToLookup(d => d.MessageType);

            InProcessDispatchers = allDispatchers
                .Where(d => d.IsInProcessHandler)
                .ToLookup(k => k.MessageType);
        }

        // Register all of the message publishers that determine how a given
        // message is delivered.  This is how the message dispatch pipeline
        // is extended.  
        public override void RegisterServices(IServiceCollection services)
        {
            foreach (Type publisherType in DispatchConfig.PublisherTypes)
            {
                services.AddScoped(typeof(IMessagePublisher), publisherType);
            }
        }

        // Registers all the message consumers within the service collection.
        public override void ScanPlugins(ITypeCatalog catalog)
        {
            catalog.AsSelf(
                t => t.IsConcreteTypeDerivedFrom<IMessageConsumer>(),
                ServiceLifetime.Scoped);
        }

        // Lookup the dispatch rules specified on the message consumer handler and
        // store a reference to the associated rule object.
        private void SetDispatchRules(IEnumerable<MessageDispatchInfo> allDispatchers)
        {
            foreach (var dispatcher in allDispatchers)
            {
                dispatcher.DispatchRules = DispatchRules
                    .Where(r => dispatcher.DispatchRuleTypes.Contains(r.GetType()))
                    .ToArray(); 
            }
        }
        
        // Check all message consumer handlers having the ApplyScriptPredicate attribute and
        // store a reference to a ScriptPredicate instance indicating the script to be executed
        // at runtime to determine if the message handler should be invoked.
        private static void SetDispatchPredicateScripts(IEnumerable<MessageDispatchInfo> allDispatchers)
        {
            foreach (var dispatcher in allDispatchers)
            {
                var scriptAttribute = dispatcher.MessageHandlerMethod.GetAttribute<ApplyScriptPredicateAttribute>();
                dispatcher.Predicate = scriptAttribute?.ToPredicate();
            }
        }

        // Assert that the dispatch rules are based on a type compatible with the 
        // message.  This applies to dispatch rules that are applied via attributes
        // since attributes can't be a constrained generic type.
        private static void AssertDispatchRules(IEnumerable<MessageDispatchInfo> allDispatchers)
        {
            var invalidEvtHandlers = allDispatchers
                .Where(d => d.DispatchRules.Any(
                    dr => !d.MessageType.CanAssignTo(dr.MessageType)))
                .Select(d => new {
                    d.MessageType,
                    d.ConsumerType,
                    d.MessageHandlerMethod.Name
                }).ToArray();
                
            if (invalidEvtHandlers.Any())
            {
                throw new ContainerException(
                    "The following message consumers have invalid rule attributes applied.  " +
                    "The handler message type and the rule message type must be assignable to each other.",
                    "InvalidHandlers", invalidEvtHandlers);
            }
        }
        
        //------------------------------------------------------
        //--Plugin Services
        //------------------------------------------------------
        
        // For a given dispatcher, creates the associated consumer and invokes it with message.
        public async Task<object> InvokeDispatcherInNewLifetimeScopeAsync(MessageDispatchInfo dispatcher, 
            IMessage message, 
            CancellationToken cancellationToken = default)
        {
            if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (cancellationToken == null) throw new ArgumentNullException(nameof(cancellationToken));

            if (! message.GetType().CanAssignTo(dispatcher.MessageType))
            {
                throw new ContainerException(
                    $"The message event type: {message.GetType()} being dispatched does not match or " +
                    $"derive from the dispatch information type of: {dispatcher.MessageType}.");
            }

            // Invoke the message consumers in a new lifetime scope.  This is for the case where a message
            // is received outside of the normal lifetime scope such as the one associated with the current
            // web request.

            using (var scope = CompositeApp.Instance.CreateServiceScope())
            {
                try
                {
                    // Resolve the component and call the message handler.
                    var consumer = (IMessageConsumer)scope.ServiceProvider.GetRequiredService(dispatcher.ConsumerType);
                    return await dispatcher.Dispatch(message, consumer, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Context.Logger.LogError(MessagingLogEvents.MessagingException, ex, "Message Dispatch Error Details.");
                    throw;
                }
            }
        }

        // For each discovered message event type, execute the same code that is used at runtime to determine
        // the consumer methods that handle the message.  Then log the information.
        public override void Log(IDictionary<string, object> moduleLog)
        {
            LogMessagesAndDispatchInfo(moduleLog);
            LogMessagePublishers(moduleLog);
        }

        private void LogMessagesAndDispatchInfo(IDictionary<string, object> moduleLog)
        {
            var messagingDispatchLog = new Dictionary<string, object>();
            moduleLog["Messaging_InProcess_Dispatchers"] = messagingDispatchLog;

            foreach (var messageTypeDispatcher in InProcessDispatchers)
            {
                var messageType = messageTypeDispatcher.Key;
                var messageDispatchers = InProcessDispatchers.WhereHandlerForMessage(messageType);

                if (messageType.FullName == null) continue;

                messagingDispatchLog[messageType.FullName] = messageDispatchers.Select(
                    ed => new
                    {
                        Consumer = ed.ConsumerType.FullName,
                        Method = ed.MessageHandlerMethod.Name,
                        ed.IsAsync,
                        IncludedDerivedTypes = ed.IncludeDerivedTypes,
                        DispatchRules = ed.DispatchRuleTypes.Select(dr => dr.FullName).ToArray(),
                        RuleApplyType = ed.DispatchRules.Any() ? ed.RuleApplyType.ToString() : null
                    }).ToArray();
            }
        }

        private void LogMessagePublishers(IDictionary<string, object> moduleLog)
        {
            moduleLog["Message_Publishers"] = Context.AllPluginTypes
                .Where(pt => pt.IsConcreteTypeDerivedFrom<IMessagePublisher>())
                .Select(t => new
                {
                    PublisherType = t.AssemblyQualifiedName,
                    IsConfigured = DispatchConfig.PublisherTypes.Contains(t)
                }).ToArray();
        }
    }
}
