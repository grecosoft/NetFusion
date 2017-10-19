namespace NetFusion.Domain.Entities.Core
{
    /// <summary>
    /// Implemented by a domain entity delegating behaviors to an internal instance.
    /// </summary>
    public interface IBehaviorDelegator
    {
        /// <summary>
        /// The entity delegated to by the domain-entity.
        /// </summary>
        IBehaviorDelegatee Behaviors { get; }

        /// <summary>
        /// Called by the factory when creating an instance of a domain entity.
        /// </summary>
        /// <param name="delegatee">The domain entity's associated delegatee.</param>
        void SetDelegatee(IBehaviorDelegatee delegatee);
    }
}
