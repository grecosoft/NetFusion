using NetFusion.Base.Plugins;
using NetFusion.Domain.Entities;

namespace NetFusion.Domain.Messaging
{
    /// <summary>
    /// Interface representing communication between a publisher and consumer.
    /// The message can also be attributed with dynamic properties.
    /// </summary>
    public interface IMessage : IKnownPluginType,
        IAttributedEntity
    {
    }
}
