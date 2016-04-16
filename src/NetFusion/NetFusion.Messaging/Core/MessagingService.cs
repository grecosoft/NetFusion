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

        public async Task PublishAsync(IDomainEvent domainEvent)
        {
            Check.NotNull(domainEvent, nameof(domainEvent), "domain event not specified");

            await PublishMessageAsync(domainEvent);
        }

        public async Task PublishAsync(ICommand command)
        {
            Check.NotNull(command, nameof(command), "command not specified");
            await PublishMessageAsync(command);
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

            foreach (var domainEvent in eventSource.DomainEvents)
            {
                try
                {
                    await PublishAsync(domainEvent);
                }
                catch (PublisherException ex)
                {
                    publisherErrors.Add(ex);
                }
            }

            if (publisherErrors.Any())
            {
                throw new PublisherException("exception dispatching event source",
                    eventSource, publisherErrors);
            }
        }

        public void PublishLocal(IDomainEvent domainEvent)
        {
            Check.NotNull(domainEvent, nameof(domainEvent), "domain event not specified");

            PublishLocal(domainEvent);
        }

        public void PublishLocal(ICommand command)
        {
            Check.NotNull(command, nameof(command), "command not specified");

            PublishLocal(command);
        }



        // Invoke all synchronous event-publishers and if there are no exceptions, execute all
        // asynchronous publishers and return the future result to the caller to await.
        private async Task PublishMessageAsync(IMessage message)
        {
            InvokeMessagePublishersSync(message);
            await InvokeMessagePublishersAsync(message);
        }

        private void PublishMessageLocal(IMessage message)
        {
            var dispatchers = _messagingModule.MessageTypeDispatchers
                .WhereHandlerForMessage(message.GetType());

            AssertAllSynchronousDispatchers(message, dispatchers);

            foreach (MessageDispatchInfo dispatcher in dispatchers)
            {
                var consumer = _lifetimeScope.Resolve(dispatcher.ConsumerType);
                dispatcher.Invoker.DynamicInvoke(consumer, message);
            }
        }

        private void InvokeMessagePublishersSync(IMessage message)
        {
            var publisherErrors = new List<PublisherException>();

            foreach (var publisher in _messagePublishers)
            {
                try
                {
                    publisher.PublishMessage(message);
                }
                catch (Exception ex)
                {
                    var publishEx = new PublisherException("error calling event publisher", publisher, ex);
                    publisherErrors.Add(publishEx);
                }
            }

            if (publisherErrors.Any())
            {
                throw new PublisherException(
                    "An exception was received when calling one or more message publishers.",
                    message,
                    publisherErrors);
            }
        }

        private async Task InvokeMessagePublishersAsync(IMessage message)
        {
            IEnumerable<MessagePublisherTask> futureResults = null;

            try
            {
                futureResults = InvokePublishersAsync(message);
                await Task.WhenAll(futureResults.Select(fr => fr.Task));
            }
            catch (Exception ex)
            {
                var publisherErrors = GetPublisherErrors(futureResults);
                if (publisherErrors.Any())
                {
                    throw new PublisherException(
                        "exception when invoking message publisher", 
                        message, 
                        publisherErrors);
                }

                throw new PublisherException(
                    "exception when invoking message publisher.",
                    message, ex);
            }
        }

        private IEnumerable<MessagePublisherTask> InvokePublishersAsync(IMessage message)
        {
            var futureResults = new List<MessagePublisherTask>();
            foreach (var publisher in _messagePublishers)
            {
                var futureResult = publisher.PublishMessageAsync(message);
                futureResults.Add(new MessagePublisherTask(futureResult, publisher));
            }
            return futureResults;
        }

        private IEnumerable<PublisherException> GetPublisherErrors(IEnumerable<MessagePublisherTask> publisherTasks)
        {
            foreach (var publisherTask in publisherTasks.Where(pt => pt.Task.Exception != null))
            {
                yield return new PublisherException(publisherTask);
            }
        }

        private void AssertAllSynchronousDispatchers(IMessage message, IEnumerable<MessageDispatchInfo> handlers)
        {
            if (handlers.Any(h => h.IsAsync))
            {
                throw new InvalidOperationException(
                    $"The message of type: {message.GetType()} has one or more asynchronous handlers " + 
                    $"and cannot be published synchronously.");
            }
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
