using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Plugin;

namespace NetFusion.Integration.ServiceBus.Internal;

/// <summary>
/// Message publisher, added to the base messaging service pipeline, used to publish
/// messages with associated Azure Service Bus namespace entities. 
/// </summary>
internal class ServiceBusPublisher : IMessagePublisher
{
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly INamespaceEntityModule _namespaceEntityModule;

    public ServiceBusPublisher(
        ILogger<ServiceBusPublisher> logger,
        INamespaceEntityModule namespaceEntityModule,
        ISerializationManager serialization)
    {
        _logger = logger;
        _namespaceEntityModule = namespaceEntityModule;
    }
    
    public IntegrationTypes IntegrationType => IntegrationTypes.External;

    public async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var messageType = message.GetType();
        
        if (! _namespaceEntityModule.TryGetPublishEntityForMessage(messageType, out BusEntity? namespaceEntity))
        {
            _logger.LogDebug("No Service Bus namespace entity for message {MessageType}", messageType);
            return;
        }

        if (namespaceEntity.TryGetPublisherStrategy(out var publishStrategy))
        {
            _logger.LogDebug("Message {MessageType} being sent to entity {EntityName} on bus {BusName}",
                messageType, 
                namespaceEntity.EntityName,
                namespaceEntity.BusName);
        
            await publishStrategy.SendToEntityAsync(message, cancellationToken);
        }
    }
}