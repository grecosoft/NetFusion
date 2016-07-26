using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Entity.Services
{
    /// <summary>
    /// Implementation responsible for evaluating a set of entity
    /// related expressions against an entity at runtime.
    /// </summary>
    public interface IEntityEvaluationService
    {
        /// <summary>
        /// Loads the entity evaluation service with a list of 
        /// expressions.
        /// </summary>
        /// <param name="expressions">List of entity related expressions.</param>
        void Load(IEnumerable<EntityPropertyExpression> expressions);

        /// <summary>
        /// Loads a set of expressions associated with the specified entity's type
        /// and evaluates them against the instance.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Evaluate<TEntity>(TEntity entity) where TEntity : IAttributedEntity;
    }
}
