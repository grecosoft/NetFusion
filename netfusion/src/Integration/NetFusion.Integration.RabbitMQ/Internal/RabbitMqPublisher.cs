using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Plugin;

namespace NetFusion.Integration.RabbitMQ.Internal;

/// <summary>
/// Extends the base messaging pipeline and publishes command and domain-events
/// for which there are defined queues and exchanges.
/// </summary>
public class RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IBusEntityModule busEntityModule) : IMessagePublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger = logger;
    private readonly IBusEntityModule _busEntityModule = busEntityModule;
    
    public IntegrationTypes IntegrationType => IntegrationTypes.External;
    
    public async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        var messageType = message.GetType();
        
        if (! _busEntityModule.TryGetPublishEntityForMessage(messageType, out BusEntity? busEntity))
        {
            _logger.LogTrace("No Bus entity for message {MessageType}", messageType);
            return;
        }

        if (busEntity.TryGetPublisherStrategy(out var publishStrategy))
        {
            await publishStrategy.SendToEntityAsync(message, cancellationToken);
        }
    }
}
