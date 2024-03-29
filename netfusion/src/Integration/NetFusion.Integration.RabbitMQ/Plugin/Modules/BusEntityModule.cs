using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Queues;

namespace NetFusion.Integration.RabbitMQ.Plugin.Modules;

public class BusEntityModule : BusEntityModuleBase<RabbitMqRouterBase>, 
    IBusEntityModule
{
    // Set by bootstrapper:
    private IBusModule BusModule { get; set; } = null!;

    public override void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IQueueResponseService, QueueResponseService>();
        base.RegisterServices(services);
    }

    protected override BusEntityContext CreateContext(IServiceProvider services) =>
        new EntityContext(Context.AppHost, services);

    protected override async Task OnStartModuleAsync(IServiceProvider services)
    {
        await base.OnStartModuleAsync(services);
        
        BusModule.Reconnection += (_, evtArgs) =>
        {
            ApplyStrategyToEntities<IBusEntityCreationStrategy>(
                s => s.CreateEntity(), 
                evtArgs.Connection.BusName).GetAwaiter().GetResult();
            
            ApplyStrategyToEntities<IBusEntitySubscriptionStrategy>(
                s => s.SubscribeEntity(), 
                evtArgs.Connection.BusName).GetAwaiter().GetResult();
        };
    }
}