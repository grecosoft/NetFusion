namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Represents a domain-entity with an identity.
    /// </summary>
    /// <typeparam name="TId">Tye type of the identity.</typeparam>
    public interface IDomainEntity<TId> 
    {
        /// <summary>
        /// The identity of the domain-entity.
        /// </summary>
        TId Id { get; }
    }
}
