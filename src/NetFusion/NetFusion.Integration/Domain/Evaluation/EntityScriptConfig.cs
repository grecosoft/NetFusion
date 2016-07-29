using NetFusion.Domain.Scripting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class EntityScriptConfig
    {
        public string Id { get; private set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<EntityExpressionConfig> Expressions { get; set; }

        public EntityScript ToEntity()
        {
            var expressions = GetEntityExpressions();

            var expressionSet = new EntityScript(
                this.Id,
                this.Name,
                this.EntityType,
                new ReadOnlyCollection<EntityExpression>(expressions));

            expressionSet.Description = this.Description;
            return expressionSet;
        }

        private IList<EntityExpression> GetEntityExpressions()
        {
            return this.Expressions.Select(e => new EntityExpression(
                e.Expression,
                e.Sequence,
                e.PropertyName) {
                    Description = e.Description

                }).ToList();
        }
    }
}
