using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Messaging.InProcess;

/// <summary>
/// Responsible for determining how messages are routed to their consumers.
/// </summary>
public interface IMessageRouter : IPluginKnownType
{
    /// <summary>
    /// Returns a dispatcher for each message describing how it is to be dispatched.
    /// </summary>
    /// <returns>List of configured dispatchers.</returns>
    public IEnumerable<MessageDispatcher> BuildMessageDispatchers();
}