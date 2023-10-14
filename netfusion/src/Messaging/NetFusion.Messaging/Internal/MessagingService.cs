using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Messaging.Internal;

/// <summary>
///  Central service for executing Commands, Domain-Events, and Queries.
/// </summary>
public class MessagingService : IMessagingService
{
    private readonly ILogger<MessagingService> _logger;
    private readonly IMessageDispatchModule _messagingModule;
    
    private readonly IEnumerable<IMessageEnricher> _messageEnrichers;
    private readonly IEnumerable<IMessageFilter> _messageFilters;
    private readonly IEnumerable<IMessagePublisher> _messagePublishers;

    public MessagingService(
        ILogger<MessagingService> logger,
        IMessageDispatchModule messagingModule,
        IEnumerable<IMessageEnricher> messageEnrichers,
        IEnumerable<IMessageFilter> messageFilters,
        IEnumerable<IMessagePublisher> messagePublishers)
    {
        _logger = logger;
        _messagingModule = messagingModule;

        // Order the enrichers, filters, publishers publishers based on the order 
        // registration specified during configuration. 
        _messageEnrichers = messageEnrichers
            .OrderByMatchingType(messagingModule.DispatchConfig.MessageEnrichers)
            .ToArray();
            
        _messageFilters = messageFilters
            .OrderByMatchingType(messagingModule.DispatchConfig.MessageFilters)
            .ToArray();

        _messagePublishers = messagePublishers
            .OrderByMatchingType(messagingModule.DispatchConfig.MessagePublishers)
            .ToArray();
    }

    public Task PublishAsync(IDomainEvent domainEvent,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default)
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent), 
            "Domain event cannot be null.");

        return PublishMessage(domainEvent, integrationType, cancellationToken);
    }

    public async Task PublishAsync(IEventSource eventSource,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default)
    {
        if (eventSource == null) throw new ArgumentNullException(nameof(eventSource),
            "Event source cannot be null.");

        var publisherErrors = new List<PublisherException>();

        foreach (IDomainEvent domainEvent in eventSource.DomainEvents)
        {
            try
            {
                await PublishMessage(domainEvent, integrationType, cancellationToken).ConfigureAwait(false);
            }
            catch (PublisherException ex)
            {
                publisherErrors.Add(ex);
            }
        }

        if (publisherErrors.Any())
        {
            throw new PublisherException("Exception dispatching event source.", eventSource, publisherErrors);
        }
    }

    public Task SendAsync(ICommand command,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default)
    {
        if (command == null) throw new ArgumentNullException(nameof(command),
            "Command cannot be null.");

        return PublishMessage(command, integrationType, cancellationToken);
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default)
    {
        if (command == null) throw new ArgumentNullException(nameof(command),
            "Command cannot be null.");

        await PublishMessage(command, integrationType, cancellationToken).ConfigureAwait(false);
        return command.Result;
    }

    public async Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query,
        CancellationToken cancellationToken = default)
    {
        if (query == null) throw new ArgumentNullException(nameof(query),
            "Query cannot be null.");

        await PublishMessage(query, IntegrationTypes.All, cancellationToken).ConfigureAwait(false);
        return query.Result;
    }

    // ----------------------------- [Publishing] -----------------------------

    // Private method to which all other publish methods delegate to asynchronously apply
    // the enrichers and invoke all registered message publishers.
    private async Task PublishMessage(IMessage message, IntegrationTypes integrationType, 
        CancellationToken cancellationToken)
    {
        try
        {
            await ApplyMessageEnrichers(message);
            await ApplyFilters<IPreMessageFilter>(message, _messageFilters, (f, m) => f.OnPreFilterAsync(m));

            LogPublishedMessage(message);

            await InvokePublishers(message, integrationType, cancellationToken);
            await ApplyFilters<IPostMessageFilter>(message, _messageFilters, (f, m) => f.OnPostFilterAsync(m));
        }
        catch (PublisherException ex)
        {
            var log = LogMessage.For(LogLevel.Error, "Exception Publishing Message {MessageType}", message.GetType())
                .WithProperties(
                    LogProperty.ForName("Message", message)
                );
                
            // Log the details of the publish exception and re-throw.
            _logger.Log(ex, log);
            throw;
        }
    }

    private async Task ApplyMessageEnrichers(IMessage message)
    {
        TaskListItem<IMessageEnricher>[]? taskList = null;
            
        try
        {
            taskList = _messageEnrichers.Invoke(message,
                (enricher, msg) => enricher.EnrichAsync(msg));

            await taskList.WhenAll();
        }
        catch (Exception ex)
        {
            if (taskList != null)
            {
                var enricherErrors = taskList.GetExceptions(GetEnricherException);
                if (enricherErrors.Any())
                {
                    throw new PublisherException("Exception when invoking message enrichers.",
                        enricherErrors);
                }
            }

            throw new PublisherException("Exception when invoking message enrichers.", ex);
        }
    }
        
    // Executes a list of asynchronous filters and awaits their completion.  Once completed,
    // any task error(s) are checked and raised.  The passed list of messages filters are
    // limited by the specified filter type of TFilter (pre/post).
    private async Task ApplyFilters<TFilter>(IMessage message, IEnumerable<IMessageFilter> filters, 
        Func<TFilter, IMessage, Task> executeFilter) where TFilter : class, IMessageFilter
    {
        var filtersByType = filters.OfType<TFilter>().ToArray();

        LogQueryFilters<TFilter>(filtersByType);
            
        TaskListItem<TFilter>[]? taskList = null;
        try
        {
            taskList = filtersByType.Invoke(message, executeFilter);
            await taskList.WhenAll();
        }
        catch (Exception ex)
        {
            if (taskList != null)
            {
                var filterErrors = taskList.GetExceptions(GetFilterException);
                if (filterErrors.Any())
                {
                    throw new PublisherException("Exception when invoking query filters.", filterErrors);
                }
            }

            throw new PublisherException("Exception when invoking query filters.", ex);
        }
    }

    private async Task InvokePublishers(IMessage message,
        IntegrationTypes integrationType, CancellationToken cancellationToken)
    {
        TaskListItem<IMessagePublisher>[]? taskList = null;

        var publishers = integrationType == IntegrationTypes.All ? _messagePublishers.ToArray() 
            : _messagePublishers.Where(p => p.IntegrationType == integrationType).ToArray();

        try
        {
            taskList = publishers.Invoke(message,
                (pub, msg) => PublishMessageWithRecovery(pub, msg, cancellationToken));

            await taskList.WhenAll();
        }
        catch (Exception ex)
        {
            if (taskList != null)
            {
                var publisherErrors = taskList.GetExceptions(GetPublisherException);
                if (publisherErrors.Any())
                {
                    throw new PublisherException("Exception when invoking message publishers.",
                        ex,
                        message,
                        publisherErrors);
                }
            }

            throw new PublisherException("Exception when invoking message publishers.", ex);
        }
    }
    
    private async Task PublishMessageWithRecovery(IMessagePublisher publisher, IMessage message,
        CancellationToken cancellationToken)
    {
        var pipeline = _messagingModule.GetPublisherResiliencePipeline(publisher.GetType());
        if (pipeline == null)
        {
            await publisher.PublishMessageAsync(message, cancellationToken).ConfigureAwait(false);
            return;
        }
        
        await pipeline.ExecuteAsync(async token =>
        {
            await publisher.PublishMessageAsync(message, token).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }
    
    
    // ------------------------- [Logging] -----------------------------
        
    private void LogPublishedMessage(IMessage message)
    {
        var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Published", message.GetType())
            .WithProperties(
                LogProperty.ForName("Message", message)
            );
            
        _logger.Log(log);
    }
        
    private void LogQueryFilters<TFilter>(IEnumerable<IMessageFilter> filters) 
        where TFilter : IMessageFilter
    {
        var log = LogMessage.For(LogLevel.Debug, "Applying {FilterType} Query Filters", typeof(TFilter).Name)
            .WithProperties(
                LogProperty.ForName("FilterTypes", filters.Select(f => f.GetType().FullName).ToArray())
            );
            
        _logger.Log(log);
    }
        
    // ------------------------- [Exceptions] --------------------------

    private static PublisherException GetPublisherException(TaskListItem<IMessagePublisher> taskItem) =>
        new("Error Invoking Publisher", taskItem.Invoker, taskItem.Task.Exception);
        
    private static EnricherException GetEnricherException(TaskListItem<IMessageEnricher> taskListItem) => 
        new("Exception Applying Enricher", taskListItem.Invoker, taskListItem.Task.Exception);
        
    private static FilterException GetFilterException<T>(TaskListItem<T> taskItem)
        where T : class, IMessageFilter => 
        new("Exception Applying Query Filter", taskItem.Invoker, taskItem.Task.Exception);
}