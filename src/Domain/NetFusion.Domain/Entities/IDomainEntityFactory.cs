
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
    }
}