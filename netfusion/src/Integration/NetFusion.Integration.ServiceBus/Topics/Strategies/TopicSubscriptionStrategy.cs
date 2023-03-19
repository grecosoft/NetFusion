using System.ComponentModel;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Topics.Metadata;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.ServiceBus.Topics.Strategies;

/// <summary>
/// Strategy invoked by a microservice to define a subscription to a topic for receiving domain-events
/// published by another microservice.  The created subscription can have a set of roles used to determine
/// if domain-events published to the topic should be delivered to the subscription. 
/// </summary>
internal class TopicSubscriptionStrategy : BusEntityStrategyBase<NamespaceEntityContext>,
    IBusEntityCreationStrategy,
    IBusEntitySubscriptionStrategy,
    IBusEntityDisposeStrategy
{
    private readonly SubscriptionEntity _subscription;
    private readonly MessageDispatcher _dispatcher;

    private ServiceBusProcessor? _subscriptionProcessor;

    public TopicSubscriptionStrategy(SubscriptionEntity subscription, MessageDispatcher dispatcher)
    {
        _subscription = subscription;
        _dispatcher = dispatcher;
    }
    
    private ILogger<TopicSubscriptionStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<TopicSubscriptionStrategy>();

    [Description("Creating Subscription to Topic for receiving Domain-Events.")]
    public Task CreateEntity()
    {
        SetSubscriptionName(_subscription.SubscriptionMeta);
        
        if (Context.IsAutoCreateEnabled || _subscription.SubscriptionMeta.IsPerServiceInstance)
        {
            var connection = Context.NamespaceModule.GetConnection(_subscription.BusName);
            var subscriptionOptions = BuildSubscription(_subscription.TopicName, _subscription.SubscriptionMeta);
            var roleOptions = _subscription.SubscriptionMeta.RuleOptions.ToArray();
            
            return connection.CreateOrUpdateSubscription(_subscription.EntityName, subscriptionOptions, roleOptions);
        }

        return Task.CompletedTask;
    }
    
    private CreateSubscriptionOptions BuildSubscription(string topicName, SubscriptionMeta subscription)
    {
        var defaults = new CreateSubscriptionOptions(topicName, subscription.SubscriptionName);
        
        var subscriptionOptions = new CreateSubscriptionOptions(topicName, subscription.SubscriptionName)
        {
            ForwardTo = subscription.ForwardTo ?? defaults.ForwardTo,
            LockDuration = subscription.LockDuration ?? defaults.LockDuration,
            RequiresSession = subscription.RequiresSession ?? defaults.RequiresSession,
            MaxDeliveryCount = subscription.MaxDeliveryCount ?? defaults.MaxDeliveryCount,
            AutoDeleteOnIdle = subscription.AutoDeleteOnIdle ?? defaults.AutoDeleteOnIdle,
            EnableBatchedOperations = subscription.EnableBatchedOperations ?? defaults.EnableBatchedOperations,
            DefaultMessageTimeToLive = subscription.DefaultMessageTimeToLive ?? defaults.DefaultMessageTimeToLive,
            
            ForwardDeadLetteredMessagesTo = subscription.ForwardDeadLetteredMessagesTo
                ?? defaults.ForwardDeadLetteredMessagesTo,
            
            DeadLetteringOnMessageExpiration = subscription.DeadLetteringOnMessageExpiration 
                ?? defaults.DeadLetteringOnMessageExpiration,
            
            EnableDeadLetteringOnFilterEvaluationExceptions = subscription.EnableDeadLetteringOnFilterEvaluationExceptions
                ?? defaults.EnableDeadLetteringOnFilterEvaluationExceptions
        };

        subscriptionOptions.AutoDeleteOnIdle = subscription.IsPerServiceInstance ? TimeSpan.FromMinutes(5) :
            subscriptionOptions.AutoDeleteOnIdle;
        
        return subscriptionOptions;
    }

    private void SetSubscriptionName(SubscriptionMeta subscriptionMeta)
    {
        var meta = _subscription.SubscriptionMeta;
        meta.SubscriptionName = meta.IsPerServiceInstance ? Guid.NewGuid().ToString() : meta.SubscriptionName;
    }
    
    [Description("Subscribing to Subscription to dispatched received Domain-Events.")]
    public Task SubscribeEntity()
    {
        var connection = Context.NamespaceModule.GetConnection(_subscription.BusName);
        
        connection.ApplyExternalSubscriptionProcessorSettings(_subscription.EntityName, 
            _subscription.SubscriptionMeta.ProcessingOptions);

        _subscription.SubscriptionMeta.ProcessingOptions.Identifier = Context.HostPlugin.PluginId;
        
        _subscriptionProcessor = connection.BusClient.CreateProcessor(
            _subscription.TopicName,
            _subscription.SubscriptionMeta.SubscriptionName, 
            _subscription.SubscriptionMeta.ProcessingOptions);
        
        _subscriptionProcessor.ProcessMessageAsync += OnMessageReceived;
        _subscriptionProcessor.ProcessErrorAsync += OnProcessingError;

        return _subscriptionProcessor.StartProcessingAsync();
    }

    private async Task OnMessageReceived(ProcessMessageEventArgs args)
    {
        IMessage message = Context.DeserializeMessage(_dispatcher, args);
        
        var msgLog = new MessageLog(LogContextType.ReceivedMessage, message);
        msgLog.SentHint("service-bus-topic-subscription");

        LogReceivedMessage(message);
        AddEntityDetailsToLog(msgLog);

        try
        {
            // Dispatch the received message to the associated in-process consumer:
            await Context.MessageDispatcher.InvokeDispatcherInNewLifetimeScopeAsync(_dispatcher, message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error dispatching message {MessageType} received on queue {QueueName} defined on {Bus}.", 
                _dispatcher.MessageType.Name,
                _subscription.EntityName,
                _subscription.BusName);
            
            msgLog.AddLogError(nameof(TopicSubscriptionStrategy), ex);
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private Task OnProcessingError(ProcessErrorEventArgs args)
    {
        Context.LogProcessError(args);
        return Task.CompletedTask;
    }
    
    public async Task OnDispose()
    {
        if (_subscriptionProcessor != null)
        {
            await _subscriptionProcessor.DisposeAsync();
        }

        if (_subscription.SubscriptionMeta.IsPerServiceInstance)
        {
            var connection = Context.NamespaceModule.GetConnection(_subscription.BusName);

            await connection.AdminClient.DeleteSubscriptionAsync(
                _subscription.TopicName, 
                _subscription.SubscriptionMeta.SubscriptionName);
        }
    }
    
    private void LogReceivedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Received from {Queue} on {Bus}",
            message.GetType(),
            _subscription.EntityName,
            _subscription.BusName).WithProperties(
            LogProperty.ForName("QueueInfo", _subscription.GetLogProperties())
        );
            
        Logger.Log(log);
    }

    private void AddEntityDetailsToLog(MessageLog msgLog)
    {
        foreach ((string key, string? value) in _subscription.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
    }
}