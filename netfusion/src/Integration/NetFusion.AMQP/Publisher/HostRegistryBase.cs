using System;
using System.Collections.Generic;
using NetFusion.AMQP.Publisher.Internal;
using NetFusion.Messaging.Types;

namespace NetFusion.AMQP.Publisher
{
    /// <summary>
    /// Base class from which application specific classes are defined to specify
    /// metadata about host defined objects (i.e. Queues/Topics).
    /// </summary>
    public abstract class HostRegistryBase : IHostRegistry
    {
        private readonly List<IHostItem> _items = new List<IHostItem>();
        
        /// <summary>
        /// The name of the configured host specified within the application's
        /// configuration settings.
        /// </summary>
        public abstract string Namespace { get; }

        /// <summary>
        /// Derived registry class overrides to specify the host defined items.
        /// </summary>
        public abstract void OnRegister();

        IEnumerable<IHostItem> IHostRegistry.GetItems()
        {
            OnRegister();
            return _items;
        }

        /// <summary>
        /// Defines a queue to which a command message can be sent.
        /// </summary>
        /// <param name="name">The name of the defined queue.</param>
        /// <typeparam name="TCommand">The command type associated with queue.</typeparam>
        /// <returns>The created queue metadata for method chaining.</returns>
        protected Queue<TCommand> AddQueue<TCommand>(string name)
            where TCommand : ICommand
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Queue name not specified.", nameof(name));
            
            var queue = new Queue<TCommand>(Namespace, name);

            _items.Add(queue);
            return queue;
        }

        /// <summary>
        /// Defines a topic to which a domain-event can be published.
        /// </summary>
        /// <param name="name">The name of the topic.</param>
        /// <typeparam name="TDomainEvent">The domain-event type associated with the topic.</typeparam>
        /// <returns>The created topic metadata for method chaining.</returns>
        protected Topic<TDomainEvent> AddTopic<TDomainEvent>(string name)
            where TDomainEvent : IDomainEvent
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Topic name not specified.", nameof(name));
            
            var topic = new Topic<TDomainEvent>(Namespace, name);
            
            _items.Add(topic);
            return topic;
        }
    }
}