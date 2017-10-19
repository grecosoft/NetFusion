using System;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Defines a behavior that can be supported by a domain-entity or aggregate.  
    /// Behaviors allow for domain specific logic to be encapsulated and associated 
    /// with a given entity.  This allows domain-entities to remain manageable as the 
    /// application grows.
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// The interface type defining the behavior.
        /// </summary>
        Type BehaviorType { get; }

        /// <summary>
        /// The concrete class implementing the behavior.
        /// </summary>
        Type ImplementationType { get; }
    }
}
