using NetFusion.Common;
using NetFusion.Domain.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Domain.Patterns.Behaviors.State
{
    /// <summary>
    /// This behavior is normally associated with the aggregate and used to track 
    /// the state of the internal domain-entities.  This information can be use,
    /// for example, by consumers such as repositories to save the aggregate.
    /// </summary>
    public class AggregateStateBehavior : IAggregateStateBehavior
    {
        private readonly IBehaviorDelegator _entity;
        private IDictionary<object, EntityState> _entityStates;

        public AggregateStateBehavior(IBehaviorDelegator entity)
        {
            _entity = entity;
            _entityStates = new Dictionary<object, EntityState>();
        }

        public void SetAdded(params object[] entities) => RecordState(entities, EntityStateTypes.Added);
        public void SetUpdated(params object[] entities) => RecordState(entities, EntityStateTypes.Updated);
        public void SetRemoved(params object[] entities) => RecordState(entities, EntityStateTypes.Removed);

        public void Clear() => _entityStates.Clear();

        public void Replace(object existingEntity, object newEntity)
        {
            Check.NotNull(existingEntity, nameof(existingEntity));
            Check.NotNull(newEntity, nameof(newEntity));

            SetRemoved(existingEntity);
            SetAdded(newEntity);
        }

        private void RecordState(object[] entities, EntityStateTypes entityStateType)
        {
            Check.NotNull(entities, nameof(entities));

            foreach (var entity in entities)
            {
                _entityStates[entity] = new EntityState(entity, entityStateType); ;
            }
        }

        public IEnumerable<EntityState> GetEntityStates()
        {
            return _entityStates.Values;
        }

        public IEnumerable<object> GetEntityStates(EntityStateTypes entityStateType)
        {
            return _entityStates.Values.Where(es => es.State == entityStateType)
                .Select(es => es.Entity);
        }

        public IEnumerable<TEntity> GetEntityStates<TEntity>(EntityStateTypes entityStateType)
            where TEntity : class
        {
            return _entityStates.Values.Where(es => es.State == entityStateType)
                .Select(es => es.Entity).OfType<TEntity>();
        }

        public void ForEach<TEntity>(EntityStateTypes entityStateType, 
            Action<TEntity> action)
            where TEntity : class
        {
            if (action == null) throw new ArgumentNullException("Action not specified.", nameof(action));

            var entitiesWithState = GetEntityStates<TEntity>(entityStateType);
            foreach (TEntity entity in entitiesWithState)
            {
                action(entity);
            }
        }
    }
}
