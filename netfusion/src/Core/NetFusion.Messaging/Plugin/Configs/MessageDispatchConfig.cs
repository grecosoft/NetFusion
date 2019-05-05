﻿using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Enrichers;

namespace NetFusion.Messaging.Plugin.Configs
{
    /// <summary>
    /// Messaging specific plug-in configurations.
    /// </summary>
    public class MessageDispatchConfig : IPluginConfig
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
            AddEnricher<CorrelationEnricher>();
            AddEnricher<DateReceivedEnricher>();
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
        public void ClearPublishers()
        {
            _messagePublisherTypes.Clear();
        }

        /// <summary>
        /// Clears any registered default message enrichers.
        /// </summary>
        public void ClearEnrichers()
        {
            _messageEnrichers.Clear();
        }

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
                throw new ContainerException(
                    $"The message publisher of type: {publisherType} is already registered.");
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
            if (_messageEnrichers.Contains(enricherType))
            {
                throw new ContainerException(
                    $"The message enricher of type: {enricherType} is already registered.");
            }

            _messageEnrichers.Add(enricherType);
        }
    }
}