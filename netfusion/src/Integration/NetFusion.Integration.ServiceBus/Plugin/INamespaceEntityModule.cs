using System.Diagnostics.CodeAnalysis;
using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.ServiceBus.Plugin;

/// <summary>
/// Module managing the initialization of namespace entities determining how
/// messages are published and received from Azure Service Bus.
/// </summary>
internal interface INamespaceEntityModule : IPluginModuleService
{
    /// <summary>
    /// Returns the namespace entity responsible for publishing a given message type
    /// to Azure Service Bus.
    /// </summary>
    /// <param name="messageType">The type of message being published.</param>
    /// <param name="entity">The registered namespace entity knowing how to publish the message type.</param>
    /// <returns>True if an entity is found.  Otherwise, False.</returns>
    bool TryGetPublishEntityForMessage(Type messageType, [NotNullWhen(true)] out BusEntity? entity);
}