using NetFusion.Integration.Bus;
using NetFusion.Integration.Redis.Internal;
using NetFusion.Integration.Redis.Publisher;
using NetFusion.Integration.Redis.Publisher.Metadata;
using NetFusion.Integration.Redis.Subscriber;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Routing;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Integration.Redis;

/// <summary>
/// Provides a fluent Api for specifying the supported messaging patterns.
/// Derived from by a microservice and called during bootstrap to define
/// the message types associated with Redis Pub/Sub entities. 
/// </summary>
public abstract class RedisRouter : BusRouterBase
{
    protected RedisRouter(string busName) : base(busName)
    {
    }

    /// <summary>
    /// Defines a channel to which a domain-event should be published.
    /// </summary>
    /// <param name="configure">The metadata specifying the channel name.</param>
    /// <typeparam name="TDomainEvent">The type of the domain-event.</typeparam>
    protected void DefineChannel<TDomainEvent>(Action<PublishMeta<TDomainEvent>> configure)
        where TDomainEvent : IDomainEvent
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));
        
        var channelMeta = new PublishMeta<TDomainEvent>();
        configure(channelMeta);

        if (string.IsNullOrWhiteSpace(channelMeta.ChannelName))
        {
            throw new RedisPluginException("Channel name must be specified.");
        }
        
        var publisher = new ChannelPublisherEntity(
            typeof(TDomainEvent),
            BusName,
            channelMeta.ChannelName,
            channelMeta);

        AssertChannelPublisher(publisher);
        AddBusEntity(publisher);
    }

    private void AssertChannelPublisher(ChannelPublisherEntity publisherEntity)
    {
        var existingDomainEvent = BusEntities.OfType<ChannelPublisherEntity>()
            .FirstOrDefault(p => p.DomainEventType == publisherEntity.DomainEventType);
    
        if (existingDomainEvent != null)
        {
            throw new RedisPluginException($"The domain-event type of: {publisherEntity.DomainEventType} is already " +
                                           $"mapped to channel: {existingDomainEvent.EntityName}");
        }
        
        var existingChannel = BusEntities.OfType<ChannelPublisherEntity>()
            .FirstOrDefault(p => p.EntityName == publisherEntity.EntityName);
    
        if (existingChannel != null)
        {
            throw new RedisPluginException($"The channel name: {publisherEntity.EntityName} is already mapped to " +
                                           $"domain event: {publisherEntity.DomainEventType}");
        }
    }

    /// <summary>
    /// Subscribes a consumer to a channel called when a domain-event is published.
    /// </summary>
    /// <param name="channelName">The name of the channel.  A pattern can be specified
    /// as part of the channel name used to filter received domain-events.</param>
    /// <param name="route">The route specifying the consumer the domain-event should be
    /// routed when published to the channel.</param>
    /// <typeparam name="TDomainEntity">The type of domain-event received on the channel.</typeparam>
    protected void SubscribeToChannel<TDomainEntity>(string channelName, 
        Action<DomainEventRoute<TDomainEntity>> route)
        where TDomainEntity : IDomainEvent
    {
        if (string.IsNullOrWhiteSpace(channelName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(channelName));

        if (route == null) throw new ArgumentNullException(nameof(route));
        
        var domainEvent = new DomainEventRoute<TDomainEntity>();
        route(domainEvent);

        var dispatcher = new MessageDispatcher(domainEvent);
        var subscriber = new ChannelSubscriberEntity(BusName, channelName, dispatcher);
        
        AssertChannelSubscriber(subscriber);
        AddBusEntity(subscriber);
    }
    
    private void AssertChannelSubscriber(ChannelSubscriberEntity subscriberEntity)
    {
        var domainEventType = subscriberEntity.Dispatcher.MessageType;
        
        var existingDomainEvent = BusEntities.OfType<ChannelSubscriberEntity>()
            .FirstOrDefault(p => p.Dispatcher.MessageType == domainEventType);
    
        if (existingDomainEvent != null)
        {
            throw new RedisPluginException($"The domain-event type of: {domainEventType} is already " +
                                           $"mapped to channel: {existingDomainEvent.EntityName}");
        }
        
        var existingChannel = BusEntities.OfType<ChannelSubscriberEntity>()
            .FirstOrDefault(p => p.EntityName == subscriberEntity.EntityName);
    
        if (existingChannel != null)
        {
            throw new RedisPluginException($"The channel name: {subscriberEntity.EntityName} is already mapped to " +
                                           $"domain event: {existingChannel.Dispatcher.MessageType}");
        }
    }
}