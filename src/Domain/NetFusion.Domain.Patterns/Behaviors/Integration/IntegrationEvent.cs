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
        private bool _isInternallyIntegrated = false;

        public IntegrationEvent(IDomainEvent domainEvent)
        {
            DomainEvent = domainEvent ?? throw new ArgumentNullException(nameof(domainEvent));
        }

        public bool IsInternallyIntegrated => _isInternallyIntegrated;

        /// <summary>
        /// Indicates that the domain-event has been published to interested consumers
        /// within the same application process.
        /// </summary>
        public void SetInternalIntegrated()
        {
            if (_isInternallyIntegrated)
            {
                throw new InvalidOperationException(
                    $"Domain Event has already been integrated with other aggregates.");
            }

            _isInternallyIntegrated = true;
        }
    }
}
