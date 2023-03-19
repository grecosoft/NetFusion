using System.Diagnostics.CodeAnalysis;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Messaging.Internal;

namespace NetFusion.Integration.Bus.Entities;

/// <summary>
/// Represents an entity defined on the service-bus used by a microservice.
/// The entity has a set of strategies invoked during bootstrap defining
/// how the entity is created, published, subscribed, and/or disposed.
/// </summary>
public abstract class BusEntity
{
    /// <summary>
    /// Name used to identify the service-bus.  This name will correspond to
    /// the settings contained within appsettings.
    /// </summary>
    public string BusName { get; }
    
    /// <summary>
    /// The name of the entity defined on the service-bus.
    /// </summary>
    public string EntityName { get; }
    
    /// <summary>
    /// Strategies associated with the entity.
    /// </summary>
    public IEnumerable<IBusEntityStrategy> Strategies { get; }
    
    private readonly List<IBusEntityStrategy> _strategies = new();

    protected BusEntity(string busName, string entityName)
    {
        if (string.IsNullOrWhiteSpace(busName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(busName));
        
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(entityName));
        
        BusName = busName;
        EntityName = entityName;
        Strategies = _strategies;
    }
    
    /// <summary>
    /// List of optional dispatchers specifying how messages, received on the service-bus entity,
    /// are dispatched to their corresponding consumer.
    /// </summary>
    public virtual IEnumerable<MessageDispatcher> Dispatchers => Array.Empty<MessageDispatcher>();
    
    /// <summary>
    /// Associates one or more strategies with the entity invoked when the microservice bootstraps.
    /// </summary>
    /// <param name="strategies">Strategy to associated when entity.</param>
    protected void AddStrategies(params IBusEntityStrategy[] strategies)
    {
        AssertSingleInstanceStrategy<IBusEntityPublishStrategy>();
        _strategies.AddRange(strategies);
    }
    
    private void AssertSingleInstanceStrategy<TStrategy>()
        where TStrategy : IBusEntityStrategy
    {
        if (_strategies.Any(s => s is TStrategy))
        {
            throw new BusException(
                $"Only one strategy of type {typeof(TStrategy)} can be added.");
        }
    }
    
    /// <summary>
    /// Returns the entity publisher strategy if supported by the entity.
    /// </summary>
    /// <param name="strategy">Reference to the publish strategy if supported.</param>
    /// <returns>True if the entity supports publishing.  Otherwise, false.</returns>
    public bool TryGetPublisherStrategy([NotNullWhen(true)] out IBusEntityPublishStrategy? strategy)
    {
        var strategies = GetStrategies<IBusEntityPublishStrategy>();
        return (strategy = strategies.FirstOrDefault()) != null;
    }
    
    /// <summary>
    /// Returns strategies of a specific type supported by the entity.
    /// </summary>
    /// <typeparam name="TStrategy">The type of strategies to find.</typeparam>
    /// <returns>List of strategies.</returns>
    public IEnumerable<TStrategy> GetStrategies<TStrategy>() 
        where TStrategy : IBusEntityStrategy => _strategies.OfType<TStrategy>();

    public IDictionary<string, string> GetLogProperties() => OnLogProperties().RemoveNullValues();
    
    protected virtual IDictionary<string, string?> OnLogProperties() => new Dictionary<string, string?>();
}