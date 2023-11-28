using NetFusion.Integration.Bus.Entities;

namespace NetFusion.Integration.Bus.Strategies;

/// <summary>
/// Base entity strategy implementation associated with a context
/// containing cross-cutting services by the strategy.
/// </summary>
/// <typeparam name="TContext">The type of context associated with strategy.</typeparam>
public abstract class BusEntityStrategyBase<TContext> : IBusEntityStrategy
    where TContext : BusEntityContext
{
    public BusEntity BusEntity { get; }
    private TContext? _context;

    protected BusEntityStrategyBase(BusEntity busEntity)
    {
        BusEntity = busEntity ?? throw new ArgumentNullException(nameof(busEntity));
    }

    public void SetContext(BusEntityContext context)
    {
        _context = (TContext)context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Context containing reference to cross-cutting services.
    /// </summary>
    protected TContext Context => _context ?? 
        throw new NullReferenceException("Context strategy not initialized");

    public virtual IDictionary<string, string> GetLog() => new Dictionary<string, string>();
}