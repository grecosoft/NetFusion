using System;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Defines a behavior that can be supported by a domain-entity.  Behaviors allow for domain specific 
    /// logic to be encapsulated and associated with a given domain-entity.  This allows domain-entities
    /// to remain manageable as the application grows.
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// The interface type defining the behavior.
        /// </summary>
        Type ContractType { get; }

        /// <summary>
        /// The concrete class implementing the behavior.
        /// </summary>
        Type BehaviorType { get; }
    }
}
