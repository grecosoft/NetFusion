using NetFusion.Base.Entity;
using NetFusion.Base.Plugins;

namespace NetFusion.Messaging.Types
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
