using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.Bus;

/// <summary>
/// Base class derived by a service-bus implementation defining the supported message
/// routing patterns.  For each supported messaging pattern, one or more BusEntity
/// instances are added.  Common messaging patterns are:
///     - Sending a command to a queue for processing.
///     - Sending a command to a queue for processing with an asynchronous response on a reply queue.
///     - Publishing a domain-event to notify one or more interested microservices.
///     - RPC message pattern.
/// </summary>
public abstract class BusRouterBase : IBusRouter
{
    public string BusName { get; }

    /// <summary>
    /// Creates a Bus-Router for a given named service-bus.
    /// </summary>
    /// <param name="busName">The name used to identity the bus and corresponds
    /// to the name specified with appsettings.</param>
    protected BusRouterBase(string busName)
    {
        if (string.IsNullOrWhiteSpace(busName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(busName));
        
        BusName = busName;
    }

    /// <summary>
    /// List of entities associated with the service-bus.
    /// </summary>
    protected List<BusEntity> BusEntities { get; } = new();
    
    IEnumerable<BusEntity> IBusRouter.GetBusEntities()
    {
        if (BusEntities.Any())
        {
            return BusEntities;
        }

        OnDefineEntities();
        return BusEntities;
    }

    /// <summary>
    /// Associates a bus-entity with the service-bus.
    /// </summary>
    /// <param name="busEntity">Entity to be added.</param>
    protected void AddBusEntity(BusEntity busEntity)
    {
        if (busEntity == null) throw new ArgumentNullException(nameof(busEntity));
        BusEntities.Add(busEntity);
    }

    /// <summary>
    /// Called during the bootstrapping of the microservice to define
    /// the service-bus related entities.
    /// </summary>
    protected abstract void OnDefineEntities();
}