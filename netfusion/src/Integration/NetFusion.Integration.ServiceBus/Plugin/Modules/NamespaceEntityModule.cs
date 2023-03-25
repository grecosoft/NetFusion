using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.ServiceBus.Namespaces;
using NetFusion.Integration.ServiceBus.Queues;

namespace NetFusion.Integration.ServiceBus.Plugin.Modules;

/// <summary>
/// Module managing the initialization of namespace entities determining how
/// messages are published and received from Azure Service Bus.
/// </summary>
internal class NamespaceEntityModule : BusEntityModuleBase,
    INamespaceEntityModule
{
    public override void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IQueueResponseService, QueueResponseService>();
        base.RegisterServices(services);
    }
    
    protected override BusEntityContext CreateContext(IServiceProvider services) =>
        new NamespaceEntityContext(Context.AppHost, services);
}