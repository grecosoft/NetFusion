using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin.Modules;

/// <summary>
/// Plug-in module that finds and caches all message related types and stores a lookup used
/// to determine what consumer component message handler should be invoked when a message
/// is published.  A message can be either a command, query or domain-event.
/// </summary>
public class MessageDispatchModule : PluginModule, 
    IMessageDispatchModule
{
    private MessageDispatchConfig? _dispatchConfig;
    private ILookup<Type, MessageDispatcher> _inProcessDispatchers; 
        
    // Discovered Properties:
    private IEnumerable<IMessageRouter> MessageRouters { get; set; }

    public MessageDispatchModule()
    {
        _inProcessDispatchers = Enumerable.Empty<MessageDispatcher>().ToLookup(e => e.MessageType);
        MessageRouters = Array.Empty<IMessageRouter>();
    }
        
    // ---------------------- [Plugin Initialization] ----------------------
        
    public MessageDispatchConfig DispatchConfig =>
        _dispatchConfig ?? throw new BootstrapException($"{nameof(DispatchConfig)} not initialized.");
        
        
    // Stores type meta-data for the message consumers that should be notified when a given message is published. 
    public override void Initialize()
    {
        _dispatchConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();

        if (MessageRouters.Count() > 1)
        {
            throw new BootstrapException(
                $"More than one class implementing: {typeof(IMessageRouter)} was found. " + 
                "there can be only on router per microservice.");
        }

        IMessageRouter? router = MessageRouters.FirstOrDefault();
        if (router != null)
        {
            _inProcessDispatchers = router.BuildMessageDispatchers()
                .ToLookup(h => h.MessageType);
        }
    }
        
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddSingleton<IMessageDispatcherService, MessageDispatcherService>();
        services.AddSingleton<IMessageLogger, MessageLogger>();
            
        RegisterMessagePublishers(services);
        RegisterMessageConsumers(services);
    }

    // Register all message publishers determining how a give message is delivered:
    private void RegisterMessagePublishers(IServiceCollection services)
    {
        foreach (Type publisherType in DispatchConfig.MessagePublishers)
        {
            services.AddScoped(typeof(IMessagePublisher), publisherType);
        }
    }

    // Automatically register any ConsumerTypes that are not abstract.  
    // If the route was configured using an interface of the consumer,
    // the implementing service is responsible for registering consumer.
    private void RegisterMessageConsumers(IServiceCollection services)
    {
        foreach (MessageDispatcher dispatchInfo in _inProcessDispatchers.Values()
            .Where(di => !di.ConsumerType.IsAbstract))
        {
            services.AddScoped(dispatchInfo.ConsumerType, dispatchInfo.ConsumerType);
        }
    }
        
    public IEnumerable<MessageDispatcher> GetMessageDispatchers(IMessage message) => 
        HandlersForMessage(message.GetType())
            .Where(di => di.MessagePredicate?.Invoke(message) ?? true);
            
    private IEnumerable<MessageDispatcher> HandlersForMessage(Type messageType) =>
        _inProcessDispatchers.Where(di => di.Key.IsAssignableFrom(messageType))
            .SelectMany(di => di)
            .Where(di => di.IncludeDerivedTypes || di.MessageType == messageType);

    // ---------------------- [Logging] ----------------------

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
        moduleLog["InProcessDispatchers"] = messagingDispatchLog;

        foreach (var messageTypeDispatcher in _inProcessDispatchers)
        {
            var messageType = messageTypeDispatcher.Key;
            var messageDispatchers = HandlersForMessage(messageType);

            if (messageType.FullName == null) continue;

            messagingDispatchLog[messageType.FullName] = messageDispatchers.Select(
                d => new
                {
                    Consumer = d.ConsumerType.FullName,
                    Method = d.MessageHandlerMethod.Name,
                    d.IsAsync,
                    IncludedDerivedTypes = d.IncludeDerivedTypes
                }).ToArray();
        }
    }

    private void LogMessagePublishers(IDictionary<string, object> moduleLog)
    {
        moduleLog["MessagePublishers"] = DispatchConfig.MessagePublishers
            .Select(t => new
            {
                PublisherType = t.AssemblyQualifiedName
            }).ToArray();
    }
}