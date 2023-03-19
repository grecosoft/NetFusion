namespace NetFusion.Integration.Bus.Strategies;

/// <summary>
/// Strategy invoked during bootstrapping used to define how an
/// entity is created on the service-bus. 
/// </summary>
public interface IBusEntityCreationStrategy : IBusEntityStrategy
{
    /// <summary>
    /// Invoked when the entity should be created on the service-bus.
    /// </summary>
    /// <returns>Future Result.</returns>
    Task CreateEntity();
}