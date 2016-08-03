using NetFusion.Domain.Scripting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetFusion.Integration.Domain.Scripting
{
    public class EntityScriptMeta
    {
        public string Id { get; private set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IDictionary<string, object> Attributes { get; set; }
        public ICollection<EntityExpressionMeta> Expressions { get; set; }
        public ICollection<string> ImportedAssemblies { get; set; }
        public ICollection<string> ImportedNamespaces { get; set; }

        public EntityScript ToEntity()
        {
            var expressions = GetEntityExpressions();

            var expressionSet = new EntityScript(
                this.Id,
                this.Name,
                this.EntityType,
                new ReadOnlyCollection<EntityExpression>(expressions));

            expressionSet.Description = this.Description;
            expressionSet.InitialAttributes = this.Attributes;
            expressionSet.ImportedAssemblies = this.ImportedAssemblies;
            expressionSet.ImportedNamespaces = this.ImportedNamespaces;
            return expressionSet;
        }

        private IList<EntityExpression> GetEntityExpressions()
        {
            return this.Expressions.Select(e => new EntityExpression(
                e.Expression,
                e.Sequence,
                e.AttributeName) {
                    Description = e.Description

                }).ToList();
        }
    }
}
