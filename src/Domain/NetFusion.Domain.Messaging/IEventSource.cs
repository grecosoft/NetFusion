using System.Collections.Generic;

namespace NetFusion.Domain.Messaging
{
    /// <summary>
    /// Implemented by an entity that can have associated domain events.
    /// This can be used to decouple domain entities from the infrastructure
    /// used to publish the event.
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// The domain events associated with the entity.
        /// </summary>
        IEnumerable<IDomainEvent> DomainEvents { get; }
    }
}
