using NetFusion.Core.Bootstrap.Container;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Types.Attributes;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Services.Messaging.Enrichers;

/// <summary>
/// Enricher that tags all messages with the name and identity
/// of the Microservice from where the message was published.
/// </summary>
public class HostEnricher(ICompositeApp compositeApp) : IMessageEnricher
{
    private readonly ICompositeApp _compositeApp = compositeApp;

    public Task EnrichAsync(IMessage message)
    {
        message.Attributes.SetStringValue("Microservice", _compositeApp.HostPlugin.Name);
        message.Attributes.SetStringValue("MicroserviceId", _compositeApp.HostPlugin.PluginId);
        return Task.CompletedTask;
    }
}