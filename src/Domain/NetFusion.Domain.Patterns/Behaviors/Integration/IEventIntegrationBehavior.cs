using NetFusion.Domain.Entities;
using NetFusion.Messaging.Types;
using System.Collections.Generic;

namespace NetFusion.Domain.Patterns.Behaviors.Integration
{
    /// <summary>
    /// Behavior that can be associated with an entity (most often the aggregate of
    /// a set of domain entities) to which it can add events for integrating with
    /// other aggregates.  The other aggregates to integrate with can be within the
    /// same micro-service as the aggregate or another micro-service communicated 
    /// with via a central event bus.  Aggregates are integrated when enlisted with
    /// the unit-of-work.
    /// </summary>
    public interface IEventIntegrationBehavior : IDomainBehavior
    {
        IEnumerable<IDomainEvent> NonIntegratedEvents { get; }

        IEnumerable<IDomainEvent> AllDomainEvents { get; }

        /// <summary>
        /// Records an integration event for publishing to other aggregates.
        /// </summary>
        /// <param name="domainEvent">The domain event published to other aggregates.</param>
        void Record(IDomainEvent domainEvent);

        /// <summary>
        /// Clears the recorded integration domain events.
        /// </summary>
        void Clear();

        /// <summary>
        /// Tags the integration event to indicate that is has been published to other
        /// interested aggregates existing within the same process.
        /// </summary>
        void MarkInternallyIntegrated();
    }
}
