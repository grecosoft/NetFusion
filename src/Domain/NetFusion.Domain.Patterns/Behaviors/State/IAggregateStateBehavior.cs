using NetFusion.Domain.Entities;
using System;
using System.Collections.Generic;

namespace NetFusion.Domain.Patterns.Behaviors.State
{
    /// <summary>
    /// Behavior used by an aggregate to record changes made to contained domain entities.  
    /// Repositories can query a given aggregate to determine if this behavior is supported
    /// and update the contained entities or underlying data model representation.
    /// </summary>
    public interface IAggregateStateBehavior : IDomainBehavior
    {
        /// <summary>
        /// Marks the specified entity(s) as added.
        /// </summary>
        /// <param name="entities">The entity to assign the added state.</param>
        void SetAdded(params object[] entities);

        /// <summary>
        /// Marks the specified entity(s) as updated.
        /// </summary>
        /// <param name="entities">The entity to assign the updated state.</param>
        void SetUpdated(params object[] entities);

        /// <summary>
        /// Marks the specified entity(s) as removed.
        /// </summary>
        /// <param name="entities">The entity to assign the removed state.</param>
        void SetRemoved(params object[] entities);

        /// <summary>
        /// Marks the specified entity(s) as replaced by marking the existing entity 
        /// as removed and setting the state of the new entity to added.
        /// </summary>
        /// <param name="existingEntity">The existing entity to mark as removed.</param>
        /// <param name="newEntity">The new entity to mark as added.</param>
        void Replace(object existingEntity, object newEntity);

        /// <summary>
        /// Clears all of the states being tracked for the aggregate.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns all of the current entity state recorded entries.
        /// </summary>
        /// <returns></returns>
        IEnumerable<EntityState> GetEntityStates();

        /// <summary>
        /// Returns all of the entities with a specified state.
        /// </summary>
        /// <param name="entityStateType">The state of the entities to return.</param>
        /// <returns>Collection of tracked entities with a specified state.</returns>
        IEnumerable<object> GetEntityStates(EntityStateTypes entityStateType);

        /// <summary>
        /// Returns all of the entities of a specific type and state.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to query.</typeparam>
        /// <param name="entityStateType">The state of the entities to return.</param>
        /// <returns>Collection of tracked entities with a specified state.</returns>
        IEnumerable<TEntity> GetEntityStates<TEntity>(EntityStateTypes entityStateType)
            where TEntity : class;

        /// <summary>
        /// Executions an action on all entities of a specific type and state.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to query.</typeparam>
        /// <param name="entityStateType">The state of the entities to return.</param>
        /// <param name="action">The action to be invoked for each entity with matching
        /// type and state.</param>
        void ForEach<TEntity>(EntityStateTypes entityStateType, Action<TEntity> action)
           where TEntity : class;
    }
}
