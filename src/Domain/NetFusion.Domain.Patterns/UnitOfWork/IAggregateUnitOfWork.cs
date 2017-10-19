﻿using NetFusion.Domain.Entities;
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task that is completed when the commit finishes.</returns>
        Task CommitAsync(IAggregate aggregate, Func<Task> commitAction,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Adds an aggregate to the unit-of-work that was updated as a result of 
        /// handling integration domain events recorded by another aggregate.
        /// When added, any unprocessed integration events for the aggregate will
        /// be published.
        /// </summary>
        /// <param name="aggregate">The aggregate to enlist within the unit-of-work.</param>
        /// <param name="cancellationToken">The optional cancellation token.</param>
        /// <returns>Task that will be completed after publishing the aggregate's associated
        /// integration events.</returns>
        Task EnlistAsync(IAggregate aggregate,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Returns an existing aggregate that has been enlisted within the unit-of-work.
        /// </summary>
        /// <typeparam name="TAggregate">The type of aggregate to return.</typeparam>
        /// <param name="predicate">The predicate to find the matching aggregate.</param>
        /// <returns>The matching aggregate or null if not found.</returns>
        TAggregate GetEnlistedAggregate<TAggregate>(Func<TAggregate, bool> predicate)
           where TAggregate : IAggregate;
    }
}