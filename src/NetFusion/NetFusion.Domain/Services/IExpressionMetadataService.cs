using NetFusion.Domain.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Domain.Services
{
    public interface IExpressionMetadataService
    {
        // Returns all of the expressions for a given entity type.
        Task<IEnumerable<EntityPropertyExpression>> ReadAll();

        // Save a list of entity property expressions.
        Task SaveExpressions(IEnumerable<EntityPropertyExpression> expressions);

        Task SaveExpression(EntityPropertyExpression expression);
    }
}
