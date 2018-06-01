using NetFusion.Base.Plugins;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Identifies component as a message consumer that should be searched when messages
    /// are published.  This search happens during the bootstrapping of the plug-in.
    /// </summary>
    public interface IMessageConsumer : IKnownPluginType
    { 
    }
}
