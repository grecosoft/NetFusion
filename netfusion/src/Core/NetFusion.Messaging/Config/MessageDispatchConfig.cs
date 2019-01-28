using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Enrichers;
using System;
using System.Collections.Generic;

namespace NetFusion.Messaging.Config
{
    /// <summary>
    /// Messaging specific plug-in configurations.
    /// </summary>
    public class MessageDispatchConfig : IContainerConfig
    {
        // The list of message publishers that can handle message delivery.  
        // The default publisher dispatches messages to in-process event handlers.
        private readonly List<Type> _messagePublisherTypes = new List<Type> { typeof(InProcessMessagePublisher) };
        private readonly List<Type> _messageEnrichers = new List<Type>();

        public MessageDispatchConfig()
        {
            PublisherTypes = _messagePublisherTypes.AsReadOnly();
            EnricherTypes = _messageEnrichers.AsReadOnly();
            
            // Default set of message enrichers.  If not desired, the host
            // application's configuration can clear.
            AddMessageEnricher<CorrelationEnricher>();
            AddMessageEnricher<DateReceivedEnricher>();
        }

        /// <summary>
        /// Publishers that will be called to deliver published messages.
        /// </summary>
        public IReadOnlyCollection<Type> PublisherTypes { get; }

        /// <summary>
        /// Enrichers that will be called before the message is published.
        /// </summary>
        public IReadOnlyCollection<Type> EnricherTypes { get; } 

        /// <summary>
        /// Clears any registered default message publishers.
        /// </summary>
        public void ClearMessagePublishers()
        {
            _messagePublisherTypes.Clear();
        }

        /// <summary>
        /// Clears any registered default message enrichers.
        /// </summary>
        public void ClearMessageEnrichers()
        {
            _messageEnrichers.Clear();
        }

        /// <summary>
        /// Adds message publisher to be executed when a message is published.  
        /// By default, the In-Process Message Publisher is registered.
        /// </summary>
        /// <typeparam name="TPublisher">The message publisher type.</typeparam>
        public void AddMessagePublisher<TPublisher>() where TPublisher: IMessagePublisher
        {
            Type publisherType = typeof(TPublisher);
            if (_messagePublisherTypes.Contains(publisherType))
            {
                throw new ContainerException(
                    $"The message publisher of type: {publisherType} is already registered.");
            }

            _messagePublisherTypes.Add(publisherType);
        }

        /// <summary>
        /// Adds message enricher to be executed before a message is published.
        /// </summary>
        /// <typeparam name="TEnricher">The type of the enricher.</typeparam>
        public void AddMessageEnricher<TEnricher>() where TEnricher : IMessageEnricher
        {
            Type enricherType = typeof(TEnricher);
            if (_messageEnrichers.Contains(enricherType))
            {
                throw new ContainerException(
                    $"The message enricher of type: {enricherType} is already registered.");
            }

            _messageEnrichers.Add(enricherType);
        }
    }
}
