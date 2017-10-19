using NetFusion.Common;
using NetFusion.Messaging.Types;
using System;

namespace NetFusion.Domain.Patterns.Behaviors.Integration
{
    /// <summary>
    /// An event used to notify other aggregates of changes occurring 
    /// within a given aggregate.
    /// </summary>
    public class IntegrationEvent
    {
        /// <summary>
        /// The associated domain-event.
        /// </summary>
        public IDomainEvent DomainEvent { get; }
        private bool _isInternalIntegrated = false;

        public IntegrationEvent(IDomainEvent domainEvent)
        {
            Check.NotNull(domainEvent, nameof(Domain));
            DomainEvent = domainEvent;
        }

        public bool IsInternalIntegrated => _isInternalIntegrated;

        /// <summary>
        /// Indicates that the domain-event has been published to interested consumers
        /// within the same application process.
        /// </summary>
        public void SetInternalIntegrated()
        {
            if (_isInternalIntegrated)
            {
                throw new InvalidOperationException(
                    $"Domain Event has already been integrated with other aggregates.");
            }

            _isInternalIntegrated = true;
        }
    }
}
