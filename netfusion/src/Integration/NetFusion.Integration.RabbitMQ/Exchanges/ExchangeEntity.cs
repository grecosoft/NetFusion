using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Exchanges.Strategies;

namespace NetFusion.Integration.RabbitMQ.Exchanges;

/// <summary>
/// Defines an exchange to which domain-events can be published to notify
/// other microservices of changes.
/// </summary>
public class ExchangeEntity : BusEntity
{
    public Type DomainEventType { get; }
    public ExchangeMeta ExchangeMeta { get; }

    public ExchangeEntity(Type domainEventType, string busName, string exchangeName, ExchangeMeta exchangeMeta) :
        base(busName, exchangeName)
    {
        DomainEventType = domainEventType;
        ExchangeMeta = exchangeMeta;

        AddStrategies(new ExchangeCreationStrategy(this));
    }

    protected override IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>
    {
        { "DomainEventType", DomainEventType.Name },
        { "BusName", BusName },
        { "ExchangeName", ExchangeMeta.ExchangeName },
        { "ExchangeType", ExchangeMeta.ExchangeType },
        { "AlternateExchangeName", ExchangeMeta.AlternateExchangeName },
        { "IsDurable", ExchangeMeta.IsDurable.ToString() },
        { "IsAutoDelete", ExchangeMeta.IsAutoDelete.ToString() },
        { "Priority", ExchangeMeta.PublishOptions.Priority?.ToString() },
        { "IsMandatory", ExchangeMeta.PublishOptions.IsMandatory.ToString() },
        { "IsPersistent", ExchangeMeta.PublishOptions.IsPersistent.ToString() }
    };
}