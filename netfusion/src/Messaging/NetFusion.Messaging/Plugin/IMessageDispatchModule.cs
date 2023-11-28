using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin.Configs;
using Polly;

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

    /// <summary>
    /// Returns the Polly resilience pipeline to be used for a specified message publisher.
    /// </summary>
    /// <param name="publisherType">The type of message publisher.</param>
    /// <returns>The resilience pipeline associated with the message publisher.
    /// If no resilience pipeline is registered for the publisher, the default
    /// pipeline is returned if configured.  If no pipelines are registered,
    /// null will be returned.</returns>
    ResiliencePipeline? GetPublisherResiliencePipeline(Type publisherType);
}