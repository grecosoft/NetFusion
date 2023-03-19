using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Publisher.Metadata;
using NetFusion.Integration.Redis.Publisher.Strategies;

namespace NetFusion.Integration.Redis.Publisher;

public class ChannelPublisherEntity : BusEntity
{
    public Type DomainEventType { get; }
    public PublishMeta PublishMeta { get; }
    
    public ChannelPublisherEntity(Type domainEventType, string busName, string channelName, 
        PublishMeta publishMeta) : 
        base(busName, channelName)
    {
        DomainEventType = domainEventType;
        PublishMeta = publishMeta ?? throw new ArgumentNullException(nameof(publishMeta));
        
        AddStrategies(new ChannelPublishStrategy(this));
    }
}