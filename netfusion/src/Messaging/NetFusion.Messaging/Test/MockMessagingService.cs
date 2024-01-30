namespace NetFusion.Messaging.Test;

/// <summary>
/// Implementation of the IMessagingService that can be passed to a dependent component that is
/// under test.  This mock implementation allows known expected response to be registered for
/// commands.  Also, the received Commands, Queries, and Domain events are recorded so they can
/// be asserted by the unit-test if needed.
/// </summary>
public class MockMessagingService : IMessagingService
{
    // Records the received requests made to the service:
    private readonly List<object> _receivedRequests = [];
        
    // Contains known responses for commands and queries.
    private readonly Dictionary<Type, object> _commandResponses = new();
    private readonly Dictionary<Type, object> _queryResponses = new();
        
    /// <summary>
    /// Contains all the received requests (Commands, Domain Events, and Queries).  This collection
    /// can be asserted to validate that a component, injecting IMessagingService, has correctly
    /// sent messages of the correct type.
    /// </summary>
    public IReadOnlyCollection<object> ReceivedRequests { get; }

    public MockMessagingService()
    {
        ReceivedRequests = _receivedRequests;
    }
        
    /// <summary>
    /// Registers a response to be returned when a command of a specific type is received.
    /// </summary>
    /// <param name="response">The expected response.</param>
    /// <typeparam name="T">The type of the command for which the response is to be returned.</typeparam>
    /// <returns>Self Reference.</returns>
    public MockMessagingService AddCommandResponse<T>(object response)
        where T : ICommand
    {
        ArgumentNullException.ThrowIfNull(response);

        Type commandType = typeof(T);
        if (!_commandResponses.TryAdd(commandType, response))
        {
            throw new InvalidOperationException(
                $"The command of type: {commandType} already has a response registered.");
        }

        return this;
    }

    /// <summary>
    /// Registers a response to be returned when a query of a specific type is received.
    /// </summary>
    /// <param name="response">The expected response.</param>
    /// <typeparam name="T">The type of the query for which the response is to be returned.</typeparam>
    /// <returns>Self Reference</returns>
    public MockMessagingService AddQueryResponse<T>(object response)
        where T : IQuery
    {
        ArgumentNullException.ThrowIfNull(response);

        Type queryType = typeof(T);
        if (!_queryResponses.TryAdd(queryType, response))
        {
            throw new InvalidOperationException(
                $"The query of type: {queryType} already has a response registered.");
        }

        return this;
    }
        
    /// <summary>
    /// Returns all received commands of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of command.</typeparam>
    /// <returns>List of matching commands.</returns>
    public IEnumerable<T> GetReceivedCommands<T>() 
        where T : ICommand => _receivedRequests.OfType<T>();
        
    /// <summary>
    /// Returns all received domain events of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of domain event.</typeparam>
    /// <returns>List of matching domain events.</returns>
    public IEnumerable<T> GetReceivedDomainEvents<T>()
        where T : IDomainEvent => _receivedRequests.OfType<T>();
        
    /// <summary>
    /// Returns all received queries of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of query.</typeparam>
    /// <returns>List of matching queries.</returns>
    public IEnumerable<T> GetReceivedQueries<T>()
        where T : IQuery => _receivedRequests.OfType<T>();
        
        
    //-- Mocked Implementation of IMessagingServices:
        
    public Task SendAsync(ICommand command, IntegrationTypes integrationType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        _receivedRequests.Add(command);
        return Task.CompletedTask;
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, IntegrationTypes integrationType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        _receivedRequests.Add(command);
            
        object response = GetCommandResponse(command);
        return Task.FromResult((TResult)response);
    }

    public Task PublishAsync(IDomainEvent domainEvent, IntegrationTypes integrationType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _receivedRequests.Add(domainEvent);
        return Task.CompletedTask;
    }

    public Task PublishBatchAsync(IEnumerable<IDomainEvent> domainEvents, IntegrationTypes integrationType = IntegrationTypes.All,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync(IEventSource eventSource, IntegrationTypes integrationType,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(eventSource);

        _receivedRequests.Add(eventSource.DomainEvents);
        return Task.CompletedTask;
    }

    public Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        _receivedRequests.Add(query);
            
        object response = GetQueryResponse(query);
        return Task.FromResult((TResult)response);
    }
        
    private object GetCommandResponse(ICommand command)
    {
        Type commandType = command.GetType();
            
        if (! _commandResponses.TryGetValue(commandType, out object? response))
        {
            throw new InvalidOperationException(
                $"The command of type: {commandType} does not have a registered response.");
        }
            
        // This allows testing the calling code for exception scenarios.
        if (response is Exception ex)
        {
            throw ex;
        }

        return response;
    }
        
    private object GetQueryResponse(IQuery query)
    {
        Type queryType = query.GetType();
            
        if (! _queryResponses.TryGetValue(queryType, out object? response))
        {
            throw new InvalidOperationException(
                $"The query of type: {queryType} does not have a registered response.");
        }
            
        // This allows testing the calling code for exception scenarios.
        if (response is Exception ex)
        {
            throw ex;
        }

        return response;
    }
}