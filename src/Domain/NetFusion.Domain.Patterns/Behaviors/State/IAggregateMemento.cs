namespace NetFusion.Domain.Patterns.Behaviors.State
{
    /// <summary>
    /// Interface implemented by an aggregate called when restoring the
    /// state of its related domain-entities.  Often called by a repository.
    /// </summary>
    public interface IAggregateMemento
    {
        /// <summary>
        /// Notifies the aggregate to load its state from its associated
        /// IAggregateStateBehavior instance.
        /// </summary>
        /// <param name="state">The state to restore aggregate from.</param>
        void RestoreFromState(object state);
    }
}
