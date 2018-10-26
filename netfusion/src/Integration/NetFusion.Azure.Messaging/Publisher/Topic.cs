using System;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Messaging.Types;

namespace NetFusion.Azure.Messaging.Publisher
{
    /// <summary>
    /// Metadata for a topic defined on a namespace.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of domain-event assocated with topic.</typeparam>
    public class Topic<TDomainEvent> : NamespaceItem<TDomainEvent>
        where TDomainEvent : IDomainEvent
    {
        private Func<TDomainEvent, bool> _predicate;
        
        public Topic(string namespaceName, string name) 
            : base(namespaceName, name)
        {
            
        }

        /// <summary>
        /// An optional predicate that can be called to determine of the
        /// domain-event should be published to the topic. 
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>True if the domain-event matches the criteria.
        /// Otherwise, false.</returns>
        public Topic<TDomainEvent> Where(Func<TDomainEvent, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
            return this;
        }

        public override bool MessageApplies(IMessage message)
        {
            return _predicate == null || _predicate((TDomainEvent) message);
        }
    }
}

