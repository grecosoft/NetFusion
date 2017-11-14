using System;

namespace NetFusion.Domain.Patterns.Behaviors.State
{
    /// <summary>
    /// Records information associated with the state of an entity.
    /// </summary>
    public class EntityState
    {
        /// <summary>
        /// The entity associated with the state.
        /// </summary>
        public object Entity { get; }

        /// <summary>
        /// The state of the entity.
        /// </summary>
        public EntityStateTypes State { get; }

        /// <summary>
        /// Optional model associated with the entity.  This can be used by consumers of
        /// the EntityState such as a repository to track an associated data model.
        /// </summary>
        public object AssociatedModel { get; private set; }

        /// <summary>
        /// Records information associated with the state of an entity.
        /// </summary>
        /// <param name="entity">The entity associated with the state.</param>
        /// <param name="state">The state of the entity.</param>
        public EntityState(object entity, EntityStateTypes state)
        {
            Entity = entity ?? throw new ArgumentNullException("Entity must be specified.", nameof(entity));
            State = state;
        }

        /// <summary>
        /// Model associated with the entity.  This can be used by consumers of the EntityState
        /// such as repository to track an associated data model.
        /// </summary>
        /// <param name="model">The associated model.</param>
        public void SetAssociatedModel(object model)
        {
            AssociatedModel = model ?? throw new ArgumentNullException("Associated model cannot be null.", nameof(model));
        }

        /// <summary>
        /// Clears the entity state's associated model.
        /// </summary>
        public void ClearAssociatedModel()
        {
            AssociatedModel = null;
        }
    }
}
