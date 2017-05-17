namespace NetFusion.Domain.Entities
{
    /// <summary>
    /// Implemented to by a domain entity and encapsulates it associated behaviors.
    /// </summary>
    public interface IEntity
    {
        bool SupportsBehavior<TBehavior>() where TBehavior : IDomainBehavior;

        /// <summary>
        /// Returns a supported domain-entity behavior.
        /// </summary>
        /// <typeparam name="TBehavior">The type of the behavior.</typeparam>
        /// <returns>Boolean value indicating if the domain-entity supports the specified behavior.
        /// If supported, a reference to the behavior is returned.</returns>
        (TBehavior instance, bool supported) GetBehavior<TBehavior>()
            where TBehavior : class, IDomainBehavior;
    }
}
