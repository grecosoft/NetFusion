using Microsoft.Extensions.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Service containing methods to publish messages to all registered message 
    /// publishers.  The message publishers are responsible for determining how
    /// the event is dispatched.  
    /// </summary>
    public class MessagingService : IMessagingService
    {
        private readonly ILogger<MessagingService> _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly IEnumerable<IMessageEnricher> _messageEnrichers;
        private readonly IEnumerable<IMessagePublisher> _messagePublishers;

        public MessagingService(
            ILogger<MessagingService> logger,
            IMessagingModule messagingModule,
            IEnumerable<IMessageEnricher> messageEnrichers,
            IEnumerable<IMessagePublisher> messagePublishers)
        {
            _logger = logger;
            _messagingModule = messagingModule;

            // Order the enrichers and the publishers based on the order of the type
            // registration specified during configuration.
            _messageEnrichers = messageEnrichers
                .OrderByMatchingType(_messagingModule.MessagingConfig.EnricherTypes)
                .ToList();

            _messagePublishers = messagePublishers
                .OrderByMatchingType(_messagingModule.MessagingConfig.PublisherTypes)
                .ToList();
        }

        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            Check.NotNull(domainEvent, nameof(domainEvent), "domain event not specified");
            return PublishMessageAsync(domainEvent, integrationType, cancellationToken);
        }

        public Task SendAsync(ICommand command, CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            Check.NotNull(command, nameof(command), "command not specified");
            return PublishMessageAsync(command, integrationType, cancellationToken);
        }

        public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            Check.NotNull(command, nameof(command), "command not specified");
            await PublishMessageAsync(command, integrationType, cancellationToken);
            return command.Result;
        }

        public async Task PublishAsync(IEventSource eventSource, CancellationToken cancellationToken = default(CancellationToken),
            IntegrationTypes integrationType = IntegrationTypes.All)
        {
            Check.NotNull(eventSource, nameof(eventSource), "event source not specified");
            var publisherErrors = new List<PublisherException>();

            foreach (IDomainEvent domainEvent in eventSource.DomainEvents)
            {
                try
                {
                    await PublishMessageAsync(domainEvent, integrationType, cancellationToken);
                }
                catch (PublisherException ex)
                {
                    publisherErrors.Add(ex);
                }
            }

            if (publisherErrors.Any())
            {
                throw new PublisherException("Exception dispatching event source.",
                    eventSource, publisherErrors);
            }
        }

        // Private method to which all other publish methods delegate to asynchronously apply
        // the enrichers and to invoke all registered message publishers.
        private async Task PublishMessageAsync(IMessage message, IntegrationTypes integrationType, CancellationToken cancellationToken)
        {
            try
            {
                await ApplyMessageEnrichers(message);
                await InvokePublishers(message, cancellationToken, integrationType);
            }
            catch (PublisherException ex)
            {
                // Log the details of the publish exception and throw a generic error messages.
                _logger.LogError(MessagingLogEvents.MESSAGING_EXCEPTION, ex, "Exception publishing message.");
                throw new PublisherException("Exception publishing message.  See log for details.");
            }
        }

        private async Task ApplyMessageEnrichers(IMessage message)
        {
            FutureResult<IMessageEnricher>[] futureResults = null;

            try
            {
                futureResults = _messageEnrichers.Invoke(message,
                    (enricher, msg) => enricher.Enrich(msg));

                await futureResults.WhenAll();
            }
            catch (Exception ex)
            {
                if (futureResults != null)
                {
                    var enricherErrors = futureResults.GetExceptions(fr => new EnricherException(fr));
                    if (enricherErrors.Any())
                    {
                        throw new PublisherException("Exception when invoking message enrichers.",
                            message,
                            enricherErrors);
                    }

                    throw new PublisherException("Exception when invoking message enrichers.",
                        message,
                        ex);
                }

                throw new PublisherException("Exception when invoking message enrichers.",
                    message,
                    ex);
            }
        }

        private async Task InvokePublishers(IMessage message, CancellationToken cancellationToken, IntegrationTypes integrationType)
        {

            FutureResult<IMessagePublisher>[] futureResults = null;

            var publishers = integrationType == IntegrationTypes.All ? _messagePublishers.ToArray() 
                : _messagePublishers.Where(p => p.IntegrationType == integrationType).ToArray();

            try
            {
                futureResults = publishers.Invoke(message,
                    (pub, msg) => pub.PublishMessageAsync(msg, cancellationToken));

                await futureResults.WhenAll();
            }
            catch (Exception ex)
            {
                if (futureResults != null)
                {
                    var publisherErrors = futureResults.GetExceptions(fr => new PublisherException(fr));
                    if (publisherErrors.Any())
                    {
                        throw new PublisherException("Exception when invoking message publishers.",
                            message,
                            publisherErrors);
                    }

                    throw new PublisherException("Exception when invoking message publishers.",
                        message, ex);
                }

                throw new PublisherException("Exception when invoking message publishers.",
                    message, ex);
            }
        }
    }
}
