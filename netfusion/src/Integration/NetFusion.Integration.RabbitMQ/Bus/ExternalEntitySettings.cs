using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Plugin.Settings;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;

namespace NetFusion.Integration.RabbitMQ.Bus;

/// <summary>
/// Responsible for applying external settings to bus entities
/// specifying details about how they are created.  Any default
/// settings specified within code are overriden.
/// </summary>
public class ExternalEntitySettings(ConnectionSettings connectionSettings)
{
    private readonly ConnectionSettings _connectionSettings = connectionSettings;

    public void ApplyExchangeSettings(string entityName, ExchangeMeta exchange)
    {
        if (!_connectionSettings.ExchangeSettings.TryGetValue(entityName, out ExchangeSettings? settings)) return;
        
        exchange.IsDurable = settings.IsDurable ?? exchange.IsDurable;
        exchange.IsAutoDelete = settings.IsAutoDelete ?? exchange.IsAutoDelete;
        exchange.AlternateExchangeName = settings.AlternateExchangeName ?? exchange.AlternateExchangeName;
        
        ApplyPublishSettings(entityName, exchange.PublishOptions);
    }

    public void ApplyQueueSettings(string entityName, QueueMeta queue)
    {
        if (!_connectionSettings.QueueSettings.TryGetValue(entityName, out QueueSettings? settings)) return;

        queue.IsDurable = settings.IsDurable ?? queue.IsDurable;
        queue.IsAutoDelete = settings.IsAutoDelete ?? queue.IsAutoDelete;
        queue.IsExclusive = settings.IsExclusive ?? queue.IsExclusive;
        queue.DeadLetterExchangeName = settings.DeadLetterExchangeName ?? queue.DeadLetterExchangeName;
        queue.PerQueueMessageTtl = settings.PerQueueMessageTtl ?? queue.PerQueueMessageTtl;
        queue.MaxPriority = settings.MaxPriority ?? queue.MaxPriority;
        queue.PrefetchCount = settings.PrefetchCount ?? queue.PrefetchCount;
    }

    public void ApplyPublishSettings(string entityName, PublishOptions publishOptions)
    {
        if (!_connectionSettings.PublishSettings.TryGetValue(entityName, out PublishSettings? settings)) return;

        publishOptions.ContentType = settings.ContentType ?? publishOptions.ContentType;
        publishOptions.IsPersistent = settings.IsPersistent ?? publishOptions.IsPersistent;
        publishOptions.Priority = settings.Priority ?? publishOptions.Priority;
        publishOptions.IsMandatory = settings.IsMandatory ?? publishOptions.IsMandatory;
    }
}