using System.Reflection;

namespace NetFusion.Messaging.Routing;

/// <summary>
/// Represents a message type and its associated consumer.
/// </summary>
internal class ConsumerRoute : MessageRoute
{
    public ConsumerRoute(Type messageType, MethodInfo consumer) 
        : base(messageType)
    {
        SetConsumer(consumer);
    }

    public ConsumerRoute(Type messageType, Type resultType, MethodInfo consumer) 
        : base(messageType, resultType)
    {
        SetConsumer(consumer);
    }
}