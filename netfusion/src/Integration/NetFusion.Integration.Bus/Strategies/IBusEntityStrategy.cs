using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.Bus.Strategies;

/// <summary>
/// Base interface form which all strategies derive.
/// </summary>
public interface IBusEntityStrategy
{
    /// <summary>
    /// Reference to the entity associated with the strategy.
    /// </summary>
    public BusEntity BusEntity { get; }
    
    /// <summary>
    /// Called during bootstrapping to associate a context with the strategy.
    /// </summary>
    /// <param name="context">The context containing cross-cutting services.</param>
    void SetContext(BusEntityContext context);
}