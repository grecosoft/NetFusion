﻿using Microsoft.Extensions.Logging;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Plugin;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.Redis.Publisher;

/// <summary>
/// Extends to NetFusion base publishing pipeline to allow publishing domain-events to Redis channels.
/// </summary>
public class RedisPublisher(
    ILogger<RedisPublisher> logger,
    IBusEntityModule busEntityModule)
    : IMessagePublisher
{
    private readonly ILogger<RedisPublisher> _logger = logger;
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