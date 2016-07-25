using NetFusion.Domain.Entity;

namespace NetFusion.Domain.Roslyn.Core
{
    /// <summary>
    /// When evaluating dynamic expression against a given domain model and its set 
    /// of optional dynamic properties, the EntityEvaluationSerivce uses this as the 
    /// execution scope.</summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityScriptScope<TEntity>
        where TEntity : IAttributedEntity
    {
        /// <summary>
        /// The static domain entity type.
        /// </summary>
        public TEntity Entity { get; }

        /// <summary>
        /// The set of dynamic properties.
        /// </summary>
        public dynamic _ { get; }

        public EntityScriptScope(TEntity entity)
        {
            this.Entity = entity;
            this._ = entity.Attributes;
        }
    }
}
