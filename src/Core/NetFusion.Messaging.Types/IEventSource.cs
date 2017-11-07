using System.Collections.Generic;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Implemented by an entity that can have associated domain events.
    /// This can be used to decouple domain entities from the infrastructure
    /// used to publish the events.
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// The domain events associated with the entity.
        /// </summary>
        IEnumerable<IDomainEvent> DomainEvents { get; }
    }
}
