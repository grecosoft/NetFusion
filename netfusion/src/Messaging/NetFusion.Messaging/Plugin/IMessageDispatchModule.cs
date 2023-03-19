using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin;

/// <summary>
/// Services exposed by the messaging module for access by other plugin modules.
/// </summary>
public interface IMessageDispatchModule : IPluginModuleService
{
    /// <summary>
    /// The associated messaging configuration.
    /// </summary>
    MessageDispatchConfig DispatchConfig { get; }

    /// <summary>
    /// Returns list of dispatchers associated with a specific message.
    /// </summary>
    /// <param name="message">The message being dispatched.</param>
    /// <returns>List of dispatchers.</returns>
    IEnumerable<MessageDispatcher> GetMessageDispatchers(IMessage message);

}