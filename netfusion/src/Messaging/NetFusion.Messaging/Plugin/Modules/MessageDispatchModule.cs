using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Routing;

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

    public MessageDispatchModule()
    {
        _inProcessDispatchers = Enumerable.Empty<MessageDispatcher>().ToLookup(e => e.MessageType);
    }
        
    // ---------------------- [Plugin Initialization] ----------------------
        
    public MessageDispatchConfig DispatchConfig =>
        _dispatchConfig ?? throw new BootstrapException($"{nameof(DispatchConfig)} not initialized.");
        
        
    // Stores type meta-data for the message consumers that should be notified when a given message is published. 
    public override void Initialize()
    {
        _dispatchConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();

        MessageRoute[] messageRoutes = Context.AllPluginTypes
            .WhereMessageConsumer()
            .SelectMessageHandlers()
            .SelectMessageRoutes()
            .ToArray();

        _inProcessDispatchers = messageRoutes.Select(r => new MessageDispatcher(r)
            {
                IncludeDerivedTypes = r.HandlerMethodInfo?.GetParameters().
                    First().HasAttribute<IncludeDerivedMessagesAttribute>() ?? false
            })
            .ToLookup(r => r.MessageType);

        LogInvalidMessageDispatchers(_inProcessDispatchers);
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
    private void RegisterMessageConsumers(IServiceCollection services)
    {
        foreach (MessageDispatcher dispatchInfo in _inProcessDispatchers.Values()
            .Where(di => !di.ConsumerType.IsAbstract))
        {
            services.AddScoped(dispatchInfo.ConsumerType, dispatchInfo.ConsumerType);
        }
    }
        
    public IEnumerable<MessageDispatcher> GetMessageDispatchers(IMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        
        return HandlersForMessage(message.GetType())
            .Where(di => di.MessagePredicate?.Invoke(message) ?? true);
    }

    private IEnumerable<MessageDispatcher> HandlersForMessage(Type messageType)
    {
        if (messageType == null) throw new ArgumentNullException(nameof(messageType));
        
        return _inProcessDispatchers.Where(di => di.Key.IsAssignableFrom(messageType))
            .SelectMany(di => di)
            .Where(di => di.IncludeDerivedTypes || di.MessageType == messageType);
    }

    // ---------------------- [Logging] ----------------------

    private void LogInvalidMessageDispatchers(ILookup<Type, MessageDispatcher> dispatchers)
    {
        bool IsSingleHandlerMessageType(Type mt) => mt.IsDerivedFrom<ICommand>() || mt.IsDerivedFrom<IQuery>();

        var noHandlerMessages = dispatchers.Where(d => IsSingleHandlerMessageType(d.Key))
            .Where(mt => !mt.Any())
            .Select(d => d.Key.FullName)
            .ToArray();

        if (noHandlerMessages.Any())
        {
            NfExtensions.Logger.Log<MessageDispatchModule>(LogLevel.Warning, 
                "Handlers not found for the following messages: {MessageTypes}", 
                string.Join(",", noHandlerMessages));
        }

        var multipleHandlerMessages = dispatchers.Where(d => IsSingleHandlerMessageType(d.Key))
            .Where(mt => mt.Count() > 1)
            .Select(d => d.Key.FullName)
            .ToArray();

        if (multipleHandlerMessages.Any())
        {
            NfExtensions.Logger.Log<MessageDispatchModule>(LogLevel.Warning,
                "Handlers not found for the following messages: {MessageTypes}", 
                string.Join(",", multipleHandlerMessages));
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