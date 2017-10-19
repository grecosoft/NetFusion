﻿namespace NetFusion.Domain.Entities.Registration
{
    /// <summary>
    /// Interface for specifying behaviors supported by domain entity type.
    /// </summary>
    public interface ISupportedBehaviors
    {
        /// <summary>
        /// Registers a behavior supported by a domain-entity.
        /// </summary>
        /// <typeparam name="TContract">The contract defining the behavior.</typeparam>
        /// <typeparam name="TBehavior">The implementation of the behavior.</typeparam>
        /// <returns>Reference to the entity's supported behaviors.</returns>
        ISupportedBehaviors Add<TBehavior, TImplementation>()
           where TBehavior : IDomainBehavior
           where TImplementation : TBehavior;
    }
}
