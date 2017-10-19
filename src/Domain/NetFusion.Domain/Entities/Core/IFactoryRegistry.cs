using NetFusion.Domain.Entities.Registration;
using System;

namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Provides the contract used to add behaviors to the factory supported by domain-entities.
    /// </summary>
    public interface IFactoryRegistry
    {
        /// <summary>
        /// Associates a set of supported behaviors with a domain-entity or aggregate.
        /// </summary>
        /// <typeparam name="TDomainEntity">The entity type with supported behaviors.</typeparam>
        /// <param name="entityBehaviors">Reference to object used to add behaviors supported by the domain-entity type.</param>
        void BehaviorsFor<TDomainEntity>(Action<ISupportedBehaviors> entityBehaviors)
           where TDomainEntity : IBehaviorDelegator;

        /// <summary>
        /// Adds a global behavior that is supported by all application domain-entities.
        /// </summary>
        /// <typeparam name="TContract">The contract defining the behavior.</typeparam>
        /// <typeparam name="TBehavior">The implementation of the behavior.</typeparam>
        /// <returns>Reference to the factory.</returns>
        IFactoryRegistry AddBehavior<TBehavior, TImplementation>()
            where TBehavior : IDomainBehavior
            where TImplementation : TBehavior;
    }
}
