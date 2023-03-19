using System.Diagnostics.CodeAnalysis;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.Redis.Plugin;

public interface IBusEntityModule : IPluginModuleService
{
    bool TryGetPublishEntityForMessage(Type messageType, [NotNullWhen(true)] out BusEntity? entity);
}