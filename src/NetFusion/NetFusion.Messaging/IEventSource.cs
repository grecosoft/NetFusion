
using System.Collections.Generic;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Implemented by an entity that can have associated
    /// domain events.
    /// </summary>
    public interface IEventSource
    {
       IEnumerable<IDomainEvent> DomainEvents { get; }
    }
} 
