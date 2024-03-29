﻿namespace NetFusion.Messaging;

/// <summary>
/// Service containing methods to send commands, dispatch queries, and
/// publish domain events to their corresponding consumers.
/// </summary>
public interface IMessagingService
{
    /// <summary>
    /// Sends a command to its associated consumer.
    /// </summary>
    /// <param name="command">The command to be sent.</param>
    /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
    /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
    /// <returns>Task result.</returns>
    Task SendAsync(ICommand command,
        IntegrationTypes integrationType = IntegrationTypes.All, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command to all associated consumer message and returns the result.</summary>
    /// <param name="command">The command to be published.</param>
    /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
    /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
    /// <returns>Task result.</returns>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a domain-event to all associated consumer message handlers.
    /// </summary>
    /// <param name="domainEvent">The event to be published.</param>
    /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
    /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
    /// <returns>Task result.</returns>
    Task PublishAsync(IDomainEvent domainEvent,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a list of domain-events in batch if the underlying message publisher
    /// supports batch publishing.  If not supported, each domain-event is published
    /// individually.
    /// </summary>
    /// <param name="domainEvents">List of domain-event to publish.</param>
    /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
    /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
    /// <returns>Task result.</returns>
    Task PublishBatchAsync(IEnumerable<IDomainEvent> domainEvents,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes domain-events associated with an event source.  
    /// </summary>
    /// <param name="eventSource">The event source having associated events.</param>
    /// <param name="integrationType">Specifies the scope to which publishers send messages to subscribers.</param>
    /// <param name="cancellationToken">Optional cancellation token passed to message handler.</param>
    /// <returns>Task result.</returns>
    Task PublishAsync(IEventSource eventSource,
        IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatches a query to its corresponding consumer.  When dispatching the query, it passes through a set
    /// of pre/post filters that can alter the query or consumer returned result.
    /// </summary>
    /// <typeparam name="TResult">The type of the query result.</typeparam>
    /// <param name="query">The query to be dispatched to its corresponding consumer.</param>
    /// <param name="cancellationToken">Optional cancellation token used to cancel the asynchronous task.</param>
    /// <returns>Task containing the result of the consumer.</returns>
    Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query,
        CancellationToken cancellationToken = default);
}