﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NetFusion.Messaging;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Test.Messaging
{
    /// <summary>
    /// Implementation of the IMessagingService that can be passed to a dependent component that is
    /// under test.  This mock implementation allows known expected response to be registered for
    /// commands.  Also, the received Command, Queries, and Domain events are recorded so they can
    /// be asserted by the unit-test if needed.
    /// </summary>
    public class MockMessagingService : IMessagingService
    {
        // Records the received requests made to the service:
        private readonly List<object> _receivedRequests = new List<object>();
        
        // Contains known responses for commands and queries.
        private readonly Dictionary<Type, object> _commandResponses = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> _queryResponses = new Dictionary<Type, object>();
        
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
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            Type commandType = typeof(T);
            if (_commandResponses.ContainsKey(commandType))
            {
                throw new InvalidOperationException(
                    $"The command of type: {commandType} already has a response registered.");
            }

            _commandResponses[commandType] = response;
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
            if (response == null) throw new ArgumentNullException(nameof(response));
            
            Type queryType = typeof(T);
            if (_queryResponses.ContainsKey(queryType))
            {
                throw new InvalidOperationException(
                    $"The query of type: {queryType} already has a response registered.");
            }

            _queryResponses[queryType] = response;
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
        
        Task IMessagingService.SendAsync(ICommand command, 
            CancellationToken cancellationToken,
            IntegrationTypes integrationType)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            
            _receivedRequests.Add(command);
            return Task.CompletedTask;
        }

        Task<TResult> IMessagingService.SendAsync<TResult>(ICommand<TResult> command, 
            CancellationToken cancellationToken,
            IntegrationTypes integrationType)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            
            _receivedRequests.Add(command);
            
            object response = GetCommandResponse(command);
            return Task.FromResult((TResult)response);
        }

        Task IMessagingService.PublishAsync(IDomainEvent domainEvent, 
            CancellationToken cancellationToken,
            IntegrationTypes integrationType)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
            
            _receivedRequests.Add(domainEvent);
            return Task.CompletedTask;
        }
        
        Task IMessagingService.PublishAsync(IEventSource eventSource, 
            CancellationToken cancellationToken,
            IntegrationTypes integrationType)
        {
            if (eventSource == null) throw new ArgumentNullException(nameof(eventSource));
            
            _receivedRequests.Add(eventSource.DomainEvents);
            return Task.CompletedTask;
        }

        Task<TResult> IMessagingService.DispatchAsync<TResult>(IQuery<TResult> query, 
            CancellationToken cancellationToken)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            
            _receivedRequests.Add(query);
            
            object response = GetQueryResponse(query);
            return Task.FromResult((TResult)response);
        }
        
        private object GetCommandResponse(ICommand command)
        {
            Type commandType = command.GetType();
            
            if (! _commandResponses.TryGetValue(commandType, out object response))
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
            
            if (! _queryResponses.TryGetValue(queryType, out object response))
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
}