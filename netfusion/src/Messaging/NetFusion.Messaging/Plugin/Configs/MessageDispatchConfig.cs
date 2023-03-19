using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.InProcess;

namespace NetFusion.Messaging.Plugin.Configs;

/// <summary>
/// Messaging specific plug-in configurations.
/// </summary>
public class MessageDispatchConfig : IPluginConfig
{
    private readonly List<Type> _messagePublisherTypes = new() { typeof(MessagePublisher) };
    private readonly List<Type> _messageEnrichersTypes = new();
    private readonly List<Type> _messageFilterTypes = new();

    public MessageDispatchConfig()
    {
        MessagePublishers = _messagePublisherTypes.AsReadOnly();
        MessageEnrichers = _messageEnrichersTypes.AsReadOnly();
        MessageFilters = _messageFilterTypes.AsReadOnly();
    }

    /// <summary>
    /// Publishers that will be called to deliver published messages.
    /// </summary>
    public IReadOnlyCollection<Type> MessagePublishers { get; }

    /// <summary>
    /// Enrichers called before the message is published.  Used to set attributes on the message.
    /// </summary>
    public IReadOnlyCollection<Type> MessageEnrichers { get; } 
        
    /// <summary>
    /// Filters called before and/or after the message is published.
    /// </summary>
    public IReadOnlyCollection<Type> MessageFilters { get; }

    /// <summary>
    /// Clears any registered default message publishers.
    /// </summary>
    public void ClearPublishers() => _messagePublisherTypes.Clear();
        
    /// <summary>
    /// Clears any registered default message enrichers.
    /// </summary>
    public void ClearEnrichers() => _messageEnrichersTypes.Clear();

    /// <summary>
    /// Clears any registered default message filters.
    /// </summary>
    public void ClearFilters() => _messageEnrichersTypes.Clear();
        
    /// <summary>
    /// Adds message publisher to be executed when a message is published.  
    /// By default, the In-Process Message Publisher is registered.
    /// </summary>
    /// <typeparam name="TPublisher">The message publisher type.</typeparam>
    public void AddPublisher<TPublisher>() where TPublisher: IMessagePublisher
    {
        Type publisherType = typeof(TPublisher);
        if (_messagePublisherTypes.Contains(publisherType))
        {
            throw new BootstrapException(
                $"The message publisher of type: {publisherType} is already registered.",
                "publisher-already-registered");
        }

        _messagePublisherTypes.Add(publisherType);
    }

    /// <summary>
    /// Adds message enricher to be executed before a message is published.
    /// </summary>
    /// <typeparam name="TEnricher">The type of the enricher.</typeparam>
    public void AddEnricher<TEnricher>() where TEnricher : IMessageEnricher
    {
        Type enricherType = typeof(TEnricher);
        if (_messageEnrichersTypes.Contains(enricherType))
        {
            throw new BootstrapException(
                $"The message enricher of type: {enricherType} is already registered.",
                "enricher-already-registered");
        }

        _messageEnrichersTypes.Add(enricherType);
    }
        
    /// <summary>
    /// Adds a message filter that should be executed when dispatching the message to the consumer.
    /// The filter can implement either or both the IPreMessageFiler or IPostMessageFiler interfaces
    /// to determine when applied.
    /// </summary>
    /// <typeparam name="TFilter">The type of the filter.</typeparam>
    public void AddFilter<TFilter>()
        where TFilter : IMessageFilter
    {
        if(_messageFilterTypes.Contains(typeof(TFilter)))
        {
            throw new BootstrapException(
                $"The message filter of type: {typeof(TFilter)} is already registered.",
                "filter-already-registered");
        }
        _messageFilterTypes.Add(typeof(TFilter));
    }
}