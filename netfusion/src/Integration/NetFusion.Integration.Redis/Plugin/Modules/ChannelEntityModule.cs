using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Internal;

namespace NetFusion.Integration.Redis.Plugin.Modules;

/// <summary>
/// Extends the base Bus Entity Module and returns a context
/// specific to Redis Pub/Sub.
/// </summary>
public class ChannelEntityModule : BusEntityModuleBase<RedisRouter>,
    IBusEntityModule
{
    protected override BusEntityContext CreateContext(IServiceProvider services) =>
        new ChannelEntityContext(Context.AppHost, services);
}