using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Subscriber.Strategies;
using NetFusion.Messaging.Internal;

namespace NetFusion.Integration.Redis.Subscriber;

public class ChannelSubscriberEntity : BusEntity
{
    public MessageDispatcher Dispatcher { get; }
    
    public ChannelSubscriberEntity(string busName, string channelName, MessageDispatcher dispatcher) :
        base(busName, channelName)
    {
        Dispatcher = dispatcher;
        
        AddStrategies(new ChannelSubscriberStrategy(this));
    }

    public override IEnumerable<MessageDispatcher> Dispatchers => new[] { Dispatcher };
}