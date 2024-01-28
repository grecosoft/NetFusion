using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Internal;

namespace NetFusion.Integration.Bus.Entities;

/// <summary>
/// Base module derived by service-bus specific implementations.  Discovers all IBusRouter
/// implementations defining how service-bus entities are created and messages routed.  All
/// service-bus defined entities are created and subscribed to by invoking the strategies
/// associated with the entity.
/// </summary>
public abstract class BusEntityModuleBase<TRouter> : PluginModule
    where TRouter : IBusRouter
{
    // Set by bootstrapper:
    protected IEnumerable<TRouter> BusMessageRouters { get; set; } = Enumerable.Empty<TRouter>();
    
    protected IEnumerable<BusEntity> BusEntities { get; set; } = Array.Empty<BusEntity>();
    
    private ILogger<BusEntityModuleBase<TRouter>> Logger => Context.LoggerFactory.CreateLogger<BusEntityModuleBase<TRouter>>();
    
    public override void Initialize()
    {
        AssertNamespaceRoutes();
        BusEntities = BusMessageRouters.SelectMany(r => r.GetBusEntities()).ToArray();
    }
    
    
    
    private void AssertNamespaceRoutes()
    {
        var duplicateNamespaces = BusMessageRouters.WhereDuplicated(r => r.BusName).ToArray();
        if (duplicateNamespaces.Length != 0)
        {
            throw new BusException(
                $"More than one derived {typeof(TRouter)} class defined for bus names " + 
                $"{string.Join(", ", duplicateNamespaces)}", "DUPLICATE_BUS_ROUTERS");
        }
    }
    
    /// <summary>
    /// Registers all entity dispatches with the dependency-injection container.  If the
    /// dispatcher's consumer type is a concrete type, it is automatically registered.
    /// Otherwise, it is assumed the developer registers the service as its implemented
    /// interface.
    /// </summary>
    /// <param name="services">The microservice's service collection.</param>
    public override void RegisterServices(IServiceCollection services)
    {
        foreach (MessageDispatcher dispatcher in BusEntities.SelectMany(ne => ne.Dispatchers).Distinct())
        {
            if (! dispatcher.ConsumerType.IsAbstract)
            {
                services.AddScoped(dispatcher.ConsumerType, dispatcher.ConsumerType);
            }
        }
    }
    
    /// <summary>
    /// Called after the module has been created and the microservice has is starting.
    /// Applies all creation and subscription strategies to defined service-bus entities.
    /// </summary>
    /// <param name="services">The created service container.</param>
    protected override async Task OnStartModuleAsync(IServiceProvider services)
    {
        SetContextOnEntities(services);
        
        await ApplyStrategyToEntities<IBusEntityCreationStrategy>(s => s.CreateEntity());
        await ApplyStrategyToEntities<IBusEntitySubscriptionStrategy>(s => s.SubscribeEntity());
    }
    
    /// <summary>
    /// Called when the microservice is being shutdown and invokes the dispose strategy
    /// on all service-bus strategies.
    /// </summary>
    /// <param name="services"></param>
    protected override async Task OnStopModuleAsync(IServiceProvider services)
    {
        await ApplyStrategyToEntities<IBusEntityDisposeStrategy>(s => s.OnDispose());
    }
    
    private void SetContextOnEntities(IServiceProvider services)
    {
        // Allow derived service-bus specific implementation to create context.
        BusEntityContext entityContext = CreateContext(services);
        
        foreach (BusEntity entity in BusEntities)
        {
            foreach (IBusEntityStrategy entityStrategy in entity.Strategies)
            {
                entityStrategy.SetContext(entityContext);   
            }
        }
    }

    /// <summary>
    /// Invoked before applying strategies to service-bus entities.  
    /// </summary>
    /// <param name="services">The service collection used to resolve common cross-cutting services.</param>
    /// <returns>Derived class should return context specific to its implementation
    /// and strategy needs.</returns>
    protected abstract BusEntityContext CreateContext(IServiceProvider services);

    protected async Task ApplyStrategyToEntities<TStrategy>(Func<TStrategy, Task> strategy, 
        string? busName = null)
        where TStrategy : IBusEntityStrategy
    {
        foreach (BusEntity entity in BusEntities.Where(e => e.BusName == busName || busName == null))
        {
            var strategies = entity.GetStrategies<TStrategy>();
            foreach(TStrategy entityStrategy in strategies)
            {
                await strategy(entityStrategy);
                LogEntityStrategy(typeof(TStrategy), entity, entityStrategy);
            }
        }
    }

    private void LogEntityStrategy(Type strategyType, BusEntity entity, IBusEntityStrategy strategy)
    {
        var entityProperties = entity.GetLogProperties();
        entityProperties["_Description"] = GetStrategyDescription(strategyType, strategy);

        var logMsg = LogMessage.For(LogLevel.Debug,
            "Applied {Strategy} to {Implementation} for {EntityName} on Bus {BusName}",
            strategyType.Name,
            strategy.GetType().Name,
            entity.EntityName,
            entity.BusName).WithProperties(LogProperty.ForName("Properties", entityProperties));
                
        Logger.Log(logMsg);
    }

    private string GetStrategyDescription(Type strategyType, IBusEntityStrategy strategy)
    {
        return string.Join(", ", strategyType.GetMethods()
            .Select(m => strategy.GetType().GetMethod(m.Name)?.GetAttribute<DescriptionAttribute>()?.Description)
            .Where(d => d != null));

    }

    /// <summary>
    /// Determines if there is a publish strategy associated with a specific message type.
    /// </summary>
    /// <param name="messageType">The type of message being published.</param>
    /// <param name="entity">The service-bus entity to which the message should be delivered.</param>
    /// <returns>True if a service-bus entity to which the message should be sent is defined.
    /// Otherwise, false is returned.</returns>
    public bool TryGetPublishEntityForMessage(Type messageType, [NotNullWhen(true)] out BusEntity? entity)
    {
        entity = BusEntities.FirstOrDefault(ne => 
            ne.TryGetPublisherStrategy(out var strategy) && strategy.CanPublishMessageType(messageType)
        );

        return entity != null;
    }

    public override void Log(IDictionary<string, object> moduleLog)
    {
        var busEntityLog = new Dictionary<string, object>();
        moduleLog["BusEntities"] = busEntityLog;
        
        foreach (BusEntity busEntity in BusEntities)
        {
            busEntityLog[$"{busEntity.BusName}:{busEntity.EntityName}"] = busEntity.GetLogProperties();
        }
    }
}