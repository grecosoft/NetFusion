using System.Collections.Generic;

namespace NetFusion.Messaging.Types.Contracts
{
    /// <summary>
    /// Implemented by an entity that can have associated domain events.
    /// This can be used to decouple domain entities from the infrastructure
    /// used to publish the events.
    /// 
    /// https://lostechies.com/jimmybogard/2014/05/13/a-better-domain-events-pattern/
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// The domain events associated with the entity.
        /// </summary>
        IEnumerable<IDomainEvent> DomainEvents { get; }
    }
}
