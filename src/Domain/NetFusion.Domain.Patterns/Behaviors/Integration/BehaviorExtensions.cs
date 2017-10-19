using NetFusion.Common;
using NetFusion.Domain.Entities;
using NetFusion.Messaging.Types;
using System.Collections.Generic;

namespace NetFusion.Domain.Patterns.Behaviors.Integration
{
    /// <summary>
    /// Extensions for adding and querying integration domain-events associated
    /// with an aggregate instance.
    /// </summary>
    public static class BehaviorExtensions
    {
        /// <summary>
        /// Adds an integration event to the aggregate.  When the aggregate is committed 
        /// or enlisted with the unit-of-work, the domain-events are used to notify other
        /// internal and external aggregates of changes.
        /// </summary>
        /// <param name="aggregate">The aggregate to add integration event.</param>
        /// <param name="domainEvent">The integration domain-event.</param>
        public static void IntegrationEvent(this IAggregate aggregate, IDomainEvent domainEvent)
        {
            Check.NotNull(aggregate, nameof(aggregate));
            aggregate.Behaviors.GetRequired<IEventIntegrationBehavior>().Record(domainEvent);
        }

        /// <summary>
        /// Returns the integration domain-events recorded for a given aggregate.
        /// </summary>
        /// <param name="aggregate">The aggregate to return the associated integration events.</param>
        /// <returns>List of integration events.</returns>
        public static IEnumerable<IDomainEvent> IntegrationEvents(this IAggregate aggregate)
        {
            Check.NotNull(aggregate, nameof(aggregate));
            return aggregate.Behaviors.GetRequired<IEventIntegrationBehavior>().DomainEvents;
        }
    }
}
