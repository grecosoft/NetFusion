using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Redis.Internal;

namespace NetFusion.Integration.Redis.Plugin.Modules;

public class ChannelEntityModule : BusEntityModuleBase,
    IBusEntityModule
{
    protected override BusEntityContext CreateContext(IServiceProvider services) =>
        new ChannelEntityContext(Context.AppHost, services);
}