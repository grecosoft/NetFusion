using Autofac;
using NetFusion.Common;
using NetFusion.Messaging.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IMessagingModule _messagingModule;
        private readonly IEnumerable<IMessagePublisher> _messagePublishers;

        public MessagingService(
            ILifetimeScope lifetimeScope,
            IMessagingModule messagingModule,
            IEnumerable<IMessagePublisher> messagePublishers)
        {
            _lifetimeScope = lifetimeScope;
            _messagingModule = messagingModule;
            _messagePublishers = GetOrderedPublishers(messagePublishers).ToList();
        }

        public Task PublishAsync(IDomainEvent domainEvent)
        {
            Check.NotNull(domainEvent, nameof(domainEvent), "domain event not specified");
            return PublishMessageAsync(domainEvent);
        }

        public Task PublishAsync(ICommand command)
        {
            Check.NotNull(command, nameof(command), "command not specified");
            return PublishMessageAsync(command);
        }

        public async Task<TResult> PublishAsync<TResult>(ICommand<TResult> command)
        {
            Check.NotNull(command, nameof(command), "command not specified");
            await PublishMessageAsync(command);
            return command.Result;
        }

        public async Task PublishAsync(IEventSource eventSource)
        {
            Check.NotNull(eventSource, nameof(eventSource), "event source not specified");
            var publisherErrors = new List<PublisherException>();

            foreach (IDomainEvent domainEvent in eventSource.DomainEvents)
            {
                try
                {
                    await PublishMessageAsync(domainEvent);
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

        // Private method to which all other publish methods delegate.
        private async Task PublishMessageAsync(IMessage message)
        {
            IEnumerable<MessagePublisherTask> futureResults = null;

            try
            {
                futureResults = InvokeMessagePublishers(message);
                await Task.WhenAll(futureResults.Select(fr => fr.Task));
            }
            catch (Exception ex)
            {
                if (futureResults != null)
                {
                    var publisherErrors = GetPublisherErrors(futureResults);
                    if (publisherErrors.Any())
                    {
                        throw new PublisherException(
                            "Exception when invoking message publishers.",
                            message,
                            publisherErrors);
                    }

                    throw new PublisherException(
                       "Exception when invoking message publishers.",
                       message, ex);
                }

                throw new PublisherException(
                    "Exception when invoking message publishers.",
                    message, ex);
            }
        }

        private IEnumerable<MessagePublisherTask> InvokeMessagePublishers(IMessage message)
        {
            var futureResults = new List<MessagePublisherTask>();
            foreach (var publisher in _messagePublishers)
            {
                Task futureResult = publisher.PublishMessageAsync(message);
                futureResults.Add(new MessagePublisherTask(futureResult, publisher));
            }
            return futureResults;
        }

        private IEnumerable<PublisherException> GetPublisherErrors(IEnumerable<MessagePublisherTask> publisherTasks)
        {
            var publisherExceptions = new List<PublisherException>();

            foreach (var publisherTask in publisherTasks.Where(pt => pt.Task.Exception != null))
            {
                publisherExceptions.Add(new PublisherException(publisherTask));
            }
            return publisherExceptions;
        }

        // Orders the dispatcher instances based on a list of filter types
        // in the correct sequence order based on the bootstrap configuration.
        private IEnumerable<IMessagePublisher> GetOrderedPublishers(
            IEnumerable<IMessagePublisher> eventPublishers)
        {
            foreach (var dispatcherType in _messagingModule.MessagingConfig.PublisherTypes)
            {
                var publisher = eventPublishers.FirstOrDefault(d => d.GetType() == dispatcherType);
                if (publisher != null)
                {
                    yield return publisher;
                }
            }
        }
    }
}
