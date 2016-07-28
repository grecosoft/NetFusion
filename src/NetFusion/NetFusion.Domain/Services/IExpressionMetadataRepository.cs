using NetFusion.Domain.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Services
{
    public interface IExpressionMetadataRepository
    {
        // Returns all of the expressions for a given entity type.

        /// <summary>
        /// Returns a list of all the configured entity expressions
        /// that are to be evaluated at runtime. 
        /// </summary>
        /// <returns>List of domain entity expressions.</returns>
        Task<IEnumerable<EntityExpressionSet>> ReadAll();

        // Save a list of entity property expressions.
        Task SaveExpressions(IEnumerable<EntityExpression> expressions);

        Task SaveExpression(EntityExpression expression);
    }
}
