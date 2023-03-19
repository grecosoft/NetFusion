using System.ComponentModel;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Topics.Metadata;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.ServiceBus.Topics.Strategies;

/// <summary>
/// Strategy invoked to create a topic to which the defining microservice
/// publishes domain-events to notify other subscribing microservices.
/// </summary>
internal class TopicCreationStrategy : BusEntityStrategyBase<NamespaceEntityContext>,
    IBusEntityCreationStrategy,
    IBusEntityPublishStrategy,
    IBusEntityDisposeStrategy
{
    private readonly TopicEntity _topicEntity;
    private ServiceBusSender? _serviceBusSender;

    public TopicCreationStrategy(TopicEntity topicEntity)
    {
        _topicEntity = topicEntity;
    }

    public bool CanPublishMessageType(Type messageType) => messageType == _topicEntity.DomainEventType;
    
    private ILogger<TopicCreationStrategy> Logger => 
        Context.LoggerFactory.CreateLogger<TopicCreationStrategy>();
    
    [Description("Creating topic to which Domain-Events can be published.")]
    public async Task CreateEntity()
    {
        var connection = Context.NamespaceModule.GetConnection(_topicEntity.BusName);
        
        CreateTopicOptions topicOptions = BuildTopicOptions(_topicEntity.EntityName, _topicEntity.TopicMeta);
        if (Context.IsAutoCreateEnabled)
        {
            await connection.CreateOrUpdateTopic(_topicEntity.EntityName, topicOptions);
        }
        
        _serviceBusSender = connection.BusClient.CreateSender(topicOptions.Name, 
            new ServiceBusSenderOptions { Identifier = Context.HostPlugin.PluginId });
    }

    private static CreateTopicOptions BuildTopicOptions(string topicName, TopicMeta topicMeta)
    {
        var defaults = new CreateTopicOptions(topicName);

        return new CreateTopicOptions(topicName)
        {
            EnablePartitioning = topicMeta.EnablePartitioning ?? defaults.EnablePartitioning,
            SupportOrdering = topicMeta.SupportOrdering ?? defaults.SupportOrdering,
            RequiresDuplicateDetection = topicMeta.RequiresDuplicateDetection ?? defaults.RequiresDuplicateDetection,
            AutoDeleteOnIdle = topicMeta.AutoDeleteOnIdle ?? defaults.AutoDeleteOnIdle,
            MaxSizeInMegabytes = topicMeta.MaxSizeInMegabytes ?? defaults.MaxSizeInMegabytes,
            DefaultMessageTimeToLive = topicMeta.DefaultMessageTimeToLive ?? defaults.DefaultMessageTimeToLive,
            DuplicateDetectionHistoryTimeWindow = topicMeta.DuplicateDetectionHistoryTimeWindow ?? defaults.DuplicateDetectionHistoryTimeWindow,
            MaxMessageSizeInKilobytes = topicMeta.MaxMessageSizeInKilobytes ?? defaults.MaxMessageSizeInKilobytes,
            EnableBatchedOperations = topicMeta.EnableBatchedOperations ?? defaults.EnableBatchedOperations
        };
    }
    
    public async Task SendToEntityAsync(IMessage message, CancellationToken cancellationToken)
    {
        if (_serviceBusSender == null) throw new NullReferenceException("Topic sender not created.");

        if (!_topicEntity.TopicMeta.DoesMessageApply(message)) return;
        
        string contentType = message.GetContentType() ?? _topicEntity.PublishOptions.ContentType;
        ServiceBusMessage busMessage = Context.CreateServiceBusMessage(contentType, message);
        
        // Allow properties to be added to the service-bus message on which other microservices can filter:
        _topicEntity.TopicMeta.ApplyMessageProperties(busMessage, message);
        
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("service-bus-publish-topic");

        LogPublishedMessage(message, busMessage.ApplicationProperties);
        AddEntityDetailsToLog(msgLog, busMessage.ApplicationProperties);

        try
        {
            await _serviceBusSender.SendMessageAsync(busMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            msgLog.AddLogError(nameof(TopicCreationStrategy), ex);
            throw;
        }
        finally
        {
            await Context.MessageLogger.LogAsync(msgLog);
        }
    }
    
    private void LogPublishedMessage(IMessage message, IDictionary<string, object> appProperties)
    {
        var log = LogMessage.For(LogLevel.Debug, "Publishing {MessageType} to {Entity} on {Bus}",
            message.GetType(),
            _topicEntity.EntityName,
            _topicEntity.BusName).WithProperties(
                LogProperty.ForName("ExchangeEntity", _topicEntity.GetLogProperties()),
                LogProperty.ForName("ApplicationProperties", appProperties)
            );
            
        Logger.Log(log);
    }
    
    private void AddEntityDetailsToLog(MessageLog msgLog, IDictionary<string, object> appProperties)
    {
        foreach ((string key, string? value) in _topicEntity.GetLogProperties())
        {
            msgLog.AddLogDetail(key, value);
        }
        
        msgLog.AddLogDetail("ApplicationProperties", appProperties.ToKeyValuePairString());
    }
    
    public async Task OnDispose()
    {
        if (_serviceBusSender != null)
        {
            await _serviceBusSender.DisposeAsync();
        }
    }
}