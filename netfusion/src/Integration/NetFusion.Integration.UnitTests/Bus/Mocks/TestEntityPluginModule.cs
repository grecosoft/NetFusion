using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.UnitTests.Bus.Mocks;

/// <summary>
/// Base plugin module derived by a specific message broker specifying the broker
/// specific context to be passed to all strategies.  The base class contains the
/// bootstrap logic common across all broker implementations. 
/// </summary>
public class TestEntityPluginModule : BusEntityModuleBase<TestBusRouterBase>
{
    public IEnumerable<TestBusRouterBase> TestBusMessageRouters => BusMessageRouters;
    public IEnumerable<BusEntity> TestBusEntities => BusEntities;
    
    protected override BusEntityContext CreateContext(IServiceProvider services) =>
        new TestBusEntityContext(Context.AppHost, services);
}

/// <summary>
/// Contains common and implementation specific services passed to strategies
/// for creating bus specific entities (queues, topics, subscriptions) based
/// on the broker being used (RabbitMQ, Azure Service Bus, Redis..).
/// </summary>
public class TestBusEntityContext : BusEntityContext
{
    public TestBusEntityContext(IPlugin hostPlugin, IServiceProvider serviceProvider) 
        : base(hostPlugin, serviceProvider)
    {
    }
}