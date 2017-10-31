using NetFusion.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Domain.Patterns.UnitOfWork
{
    /// <summary>
    /// Implements the process of saving an aggregate and notifying other internal and
    /// external aggregates of changes.
    /// </summary>
    public interface IAggregateUnitOfWork
    {
        /// <summary>
        /// Saves any changes to the aggregate and integrates with other aggregates
        /// by publishing any recoded domain-events.
        /// </summary>
        /// <param name="aggregate">The aggregate to be committed.</param>
        /// <param name="commitAction">The action used to commit changes to the aggregate.</param>
        /// <param name="settings">Settings used to specify of the unit-of-work is committed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task that is completed when the commit finishes.</returns>
        Task<CommitResult> CommitAsync(IAggregate aggregate, Func<Task> commitAction,
            CommitSettings settings = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Adds an aggregate to the unit-of-work that was updated as a result of 
        /// handling integration domain events recorded by another aggregate.
        /// When added, any unprocessed integration events for the aggregate will
        /// be published.
        /// </summary>
        /// <param name="aggregate">The aggregate to enlist within the unit-of-work.</param>
        /// <param name="commitAction">The action used to commit changes to the aggregate. 
        /// Optional if the commit-action passed to the CommitAsync method saves all changes
        /// made to all enlisted aggregates.  This is the case when using a technology such
        /// as Entity-Framework.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Task that will be completed after publishing the aggregate's associated
        /// integration events.</returns>
        Task EnlistAsync(IAggregate aggregate,
            Func<Task> commitAction = null,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns an existing aggregate that has been enlisted within the unit-of-work.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate to return.</typeparam>
        /// <param name="predicate">The predicate to find the matching aggregate.</param>
        /// <returns>The matching aggregate or null if not found.</returns>
        TAggregate GetEnlistedAggregate<TAggregate>(Func<TAggregate, bool> predicate)
           where TAggregate : IAggregate;

        /// <summary>
        /// Clears the enlisted aggregates and any unit-of-work related aggregate data.
        /// This method is also called when the CommitResult returned instance is disposed.
        /// </summary>
        void Clear();
    }
}
