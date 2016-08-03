using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Domain.Entity;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Interface representing communication between a 
    /// publisher and consumer.
    /// </summary>
    public interface IMessage : IKnownPluginType,
        IAttributedEntity
    {
    }
}
