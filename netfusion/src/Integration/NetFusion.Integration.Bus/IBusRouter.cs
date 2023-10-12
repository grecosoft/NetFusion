using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.Bus;

/// <summary>
/// Responsible for defining the service-bus related entities
/// and how messages are routed.
/// </summary>
public interface IBusRouter : IPluginKnownType
{
    /// <summary>
    /// The name of the service-bus to which the entities are associated.
    /// </summary>
    public string BusName { get; }
    
    /// <summary>
    /// Returns the entities associated with the service-bus.
    /// </summary>
    /// <returns>List of bus entities.</returns>
    IEnumerable<BusEntity> GetBusEntities();
}