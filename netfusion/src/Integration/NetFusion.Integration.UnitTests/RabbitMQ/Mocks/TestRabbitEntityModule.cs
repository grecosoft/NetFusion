using NetFusion.Integration.Bus;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.RabbitMQ.Plugin.Modules;

namespace NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

public class TestRabbitEntityModule : BusEntityModule
{
    // Set by bootstrapper:
    protected IEnumerable<BusRouterBase> Routers => BusMessageRouters;

    public IEnumerable<BusEntity> Entities => BusEntities;
}