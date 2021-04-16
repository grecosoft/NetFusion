using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    /// Contains methods to publish messages to all registered message publishers.
    /// The message publishers are responsible for determining how the message is
    /// dispatched.  
    /// </summary>
    public class MessageDispatcher 
    {
        private readonly ILogger<MessageDispatcher> _logger;
        private readonly IEnumerable<IMessageEnricher> _messageEnrichers;
        private readonly IEnumerable<IMessagePublisher> _messagePublishers;

        public MessageDispatcher(
            ILogger<MessageDispatcher> logger,
            IMessageDispatchModule messagingModule,
            IEnumerable<IMessageEnricher> messageEnrichers,
            IEnumerable<IMessagePublisher> messagePublishers)
        {
            _logger = logger;

            // Order the enrichers and the publishers based on the order of the type
            // registration specified during configuration.  Order should never matter
            // but this will make the order known when debugging.
            _messageEnrichers = messageEnrichers
                .OrderByMatchingType(messagingModule.DispatchConfig.EnricherTypes)
                .ToArray();

            _messagePublishers = messagePublishers
                .OrderByMatchingType(messagingModule.DispatchConfig.PublisherTypes)
                .ToArray();
        }

        public async Task PublishAsync(IDomainEvent domainEvent,
            IntegrationTypes integrationType = IntegrationTypes.All,
            CancellationToken cancellationToken = default)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent), 
                "Domain event cannot be null.");

            await PublishMessage(domainEvent, integrationType, cancellationToken).ConfigureAwait(false);
        }

        public async Task SendAsync(ICommand command,
            IntegrationTypes integrationType = IntegrationTypes.All,
            CancellationToken cancellationToken = default)
        {
            if (command == null) throw new ArgumentNullException(nameof(command),
                "Command cannot be null.");

            await PublishMessage(command, integrationType, cancellationToken).ConfigureAwait(false);
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
        
        // ----------------------------- [Publishing] -----------------------------

        // Private method to which all other publish methods delegate to asynchronously apply
        // the enrichers and to invoke all registered message publishers.
        private async Task PublishMessage(IMessage message, IntegrationTypes integrationType, 
            CancellationToken cancellationToken)
        {
            try
            {
                await ApplyMessageEnrichers(message);
                LogPublishedMessage(message);
                
                await InvokePublishers(message, integrationType, cancellationToken);
            }
            catch (PublisherException ex)
            {
                var log = LogMessage.For(LogLevel.Error, "Exception Publishing Message {MessageType}", message.GetType())
                    .WithProperties(
                        new LogProperty { Name = "Message", Value = message }
                    );
                
                // Log the details of the publish exception and rethrow.
                _logger.Log(ex, log);
                throw;
            }
        }

        private async Task ApplyMessageEnrichers(IMessage message)
        {
            TaskListItem<IMessageEnricher>[] taskList = null;
            
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
                    var enricherErrors = taskList.GetExceptions(ti => new EnricherException(ti));
                    if (enricherErrors.Any())
                    {
                        throw new PublisherException("Exception when invoking message enrichers.",
                            enricherErrors);
                    }
                }

                throw new PublisherException("Exception when invoking message enrichers.", ex);
            }
        }

        private async Task InvokePublishers(IMessage message,
            IntegrationTypes integrationType, CancellationToken cancellationToken)
        {
            TaskListItem<IMessagePublisher>[] taskList = null;

            var publishers = integrationType == IntegrationTypes.All ? _messagePublishers.ToArray() 
                : _messagePublishers.Where(p => p.IntegrationType == integrationType).ToArray();

            try
            {
                taskList = publishers.Invoke(message,
                    (pub, msg) => pub.PublishMessageAsync(msg, cancellationToken));

                await taskList.WhenAll();
            }
            catch (Exception ex)
            {
                if (taskList != null)
                {
                    var publisherErrors = taskList.GetExceptions(ti => new PublisherException(ti));
                    if (publisherErrors.Any())
                    {
                        throw new PublisherException("Exception when invoking message publishers.",
                            message,
                            publisherErrors);
                    }
                }

                throw new PublisherException("Exception when invoking message publishers.", ex);
            }
        }
        
        private void LogPublishedMessage(IMessage message)
        {
            var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Published", message.GetType())
                .WithProperties(
                    new LogProperty { Name = "Message", Value = message }
                );
            
            _logger.Log(log);
        }
    }
}
