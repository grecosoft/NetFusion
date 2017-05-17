namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Implemented by a domain entity delegating behaviors to an internal entity instance.
    /// This allows behaviors to be associated with a domain entity encapsulating a set of
    /// related domain logic.  This keeps the domain entity from becoming complex as the 
    /// business needs change and new behaviors need to be implemented in the domain.  The 
    /// domain entity should only contain logic that is core to the domain-entity.
    /// </summary>
    public interface IEntityDelegator
    {
        /// <summary>
        /// The entity delegated to by the domain-entity.
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// Called by the factory when creating an instance of a domain entity.
        /// </summary>
        /// <param name="entity">The domain entity's associated entity.</param>
        void SetEntity(IEntity entity);
    }

    public interface IEntityDelegator<TAccessor> : IEntityDelegator
        where TAccessor : IEntityAccessor
    {
       
    }
}
