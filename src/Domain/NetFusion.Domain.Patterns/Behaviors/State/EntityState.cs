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
        /// Optional data model associated with the entity if the underlying data
        /// model is different from the domain entity.
        /// </summary>
        public object DataModel { get; private set; }

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
        /// Used to specify the data model associated with the domain entity.  This can be used if
        /// the underlying data model is different from the domain entity.
        /// </summary>
        /// <param name="dataModel"></param>
        public void SetDataModel(object dataModel)
        {
            DataModel = dataModel ?? throw new ArgumentNullException("Data model must be specified.", nameof(dataModel));
        }
    }
}
