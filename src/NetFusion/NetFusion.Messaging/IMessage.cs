using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Interface representing communication between a 
    /// publisher and consumer.
    /// </summary>
    public interface IMessage : IKnownPluginType,
        IObjectAttributes 
    {
    }
}
