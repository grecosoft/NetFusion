namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Manages a set of entity related behaviors for a domain-entity or aggregate.
    /// </summary>
    public interface IBehaviorDelegatee
    {
        /// <summary>
        /// Determines if the delegating entity supports a specific behavior.
        /// </summary>
        /// <typeparam name="TBehavior">The interface type defined the behavior.</typeparam>
        /// <returns>True if the entity supports the behavior.  Otherwise, False.</returns>
        bool Supports<TBehavior>() where TBehavior : IDomainBehavior;

        /// <summary>
        /// Returns a supported domain-entity behavior.
        /// </summary>
        /// <typeparam name="TBehavior">The type of the behavior.</typeparam>
        /// <returns>Boolean value indicating if the domain-entity supports the specified
        /// behavior.  If supported, a reference to the behavior is returned.
        /// </returns>
        (TBehavior instance, bool supported) Get<TBehavior>()
            where TBehavior : IDomainBehavior;

        /// <summary>
        /// Returns a required behavior.  If the entity does not support the specified
        /// behavior, an exception is thrown.
        /// </summary>
        /// <typeparam name="TBehavior">The type of the behavior to receive.</typeparam>
        /// <returns>The supported behavior or an exception if not supported.</returns>
        TBehavior GetRequired<TBehavior>() where TBehavior : IDomainBehavior;
    }
}
