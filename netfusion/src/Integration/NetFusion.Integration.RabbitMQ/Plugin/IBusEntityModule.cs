using System.Diagnostics.CodeAnalysis;
using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.RabbitMQ.Plugin;

public interface IBusEntityModule : IPluginModuleService
{
    bool TryGetPublishEntityForMessage(Type messageType, [NotNullWhen(true)] out BusEntity? entity);
}