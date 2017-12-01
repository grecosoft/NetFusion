using NetFusion.Common.Extensions.Collections;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Patterns.Behaviors.Integration
{
    /// <summary>
    /// Manages a collection of events recorded by an aggregate used to
    /// notify other aggregates internal or external to the micro-service.
    /// The recorded integration events are published to other aggregates 
    /// when it is committed or enlisted with the unit-of-work.
    /// </summary>
    public class EventIntegrationBehavior : IEventIntegrationBehavior
    {
        private readonly List<IntegrationEvent> _integrationEvents = new List<IntegrationEvent>();

        public IEnumerable<IntegrationEvent> IntegrationEvents { get; }

        // Returns all integration events for which internal-integration has not been completed.
        public IEnumerable<IDomainEvent> DomainEvents => _integrationEvents
            .Where(ie => !ie.IsInternallyIntegrated)
            .Select(ie => ie.DomainEvent);

        public EventIntegrationBehavior()
        {
            IntegrationEvents = _integrationEvents;
        }

        public void Record(IDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            _integrationEvents.Add(new IntegrationEvent(domainEvent));
        }

        // Updates the status of all integration events to indicate that
        // they have been integrated with aggregates located within the
        // same micro-service.
        public void MarkInternallyIntegrated()
        {
            _integrationEvents.Where(ie => !ie.IsInternallyIntegrated)
                .ForEach(ie => ie.SetInternalIntegrated());
        }

        public void Clear() => _integrationEvents.Clear();
    }
}
