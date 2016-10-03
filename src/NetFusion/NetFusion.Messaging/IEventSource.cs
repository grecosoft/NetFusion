
using System.Collections.Generic;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Implemented by an entity that can have associated
    /// domain events.
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// The domain events associated with the entity.
        /// </summary>
        IEnumerable<IDomainEvent> DomainEvents { get; }
    }
} 
