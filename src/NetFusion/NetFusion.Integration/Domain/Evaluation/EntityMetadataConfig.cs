using NetFusion.Domain.Entity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class EntityMetadataConfig
    {
        public string Id { get; private set; }
        public string EntityType { get; set; }
        public string ContextIdentifier { get; set; }
        public ICollection<ExpressionMetadataConfig> Expressions { get; set; }

        public EntityExpressionSet ToEntity()
        {
            var expressions = GetEntityExpressions();

            return new EntityExpressionSet(
                this.Id,
                this.EntityType,
                this.ContextIdentifier,
                new ReadOnlyCollection<EntityExpression>(expressions));
        }

        private IList<EntityExpression> GetEntityExpressions()
        {
            return this.Expressions.Select(e => new EntityExpression(
                e.Expression, 
                e.Sequence, 
                e.IsPersistant, 
                e.PropertyName)).ToList();
        }
    }
}
