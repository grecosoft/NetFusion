using NetFusion.Domain.Entities.Core;

namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Implemented by a factory responsible for creating domain-entities.
    /// </summary>
    public interface IDomainEntityFactory
    {
        /// <summary>
        /// Creates an instance of a domain-entity with its supported behaviors.
        /// </summary>
        /// <typeparam name="TDomainEntity">The domain entity type to be created.</typeparam>
        /// <returns>Instance of the domain entity.</returns>
        TDomainEntity Create<TDomainEntity>() where TDomainEntity : IBehaviorDelegator, new();

        /// <summary>
        /// Builds an instance of a domain-entity with its supported behaviors by delegating 
        /// the creation logic to an entity-constructor.  
        /// </summary>
        /// <typeparam name="TDomainEntity">The domain entity type to be created.</typeparam>
        /// <returns>Returns an instance of an entity-constructor.</returns>
        DomainEntityConstructor<TDomainEntity> Build<TDomainEntity>()
            where TDomainEntity : IBehaviorDelegator;

        /// <summary>
        /// Applies the factory build logic to an existing domain-entity instance.
        /// </summary>
        /// <param name="entity">The existing domain-entity reference.</param>
        void Build(IBehaviorDelegator entity);
    }
}