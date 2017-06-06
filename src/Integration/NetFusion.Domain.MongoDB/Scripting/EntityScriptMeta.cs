using NetFusion.Domain.Scripting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetFusion.Domain.MongoDB.Scripting
{
    public class EntityScriptMeta
    {
        public string ScriptId { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IDictionary<string, object> Attributes { get; set; }
        public ICollection<EntityExpressionMeta> Expressions { get; set; }
        public ICollection<string> ImportedAssemblies { get; set; }
        public ICollection<string> ImportedNamespaces { get; set; }

        public EntityScriptMeta()
        {

        }

        public EntityScriptMeta(EntityScript script)
        {
            this.ScriptId = script.ScriptId;
            this.EntityType = script.EntityType.AssemblyQualifiedName;
            this.Name = script.Name;
            this.Description = script.Description;

            this.Attributes = script.InitialAttributes;
            this.ImportedAssemblies = script.ImportedAssemblies;
            this.ImportedNamespaces = script.ImportedNamespaces;

            SetExpressions(script.Expressions);
        }

        private void SetExpressions(IEnumerable<EntityExpression> expressions)
        {
            var expressionMetadata = new List<EntityExpressionMeta>();
            foreach (EntityExpression expression in expressions)
            {
                var expressionMeta = new EntityExpressionMeta
                {
                    Expression = expression.Expression,
                    Sequence = expression.Sequence,
                    AttributeName = expression.AttributeName,
                    Description = expression.Description
                };

                expressionMetadata.Add(expressionMeta);
            }

            this.Expressions = expressionMetadata;
        }

        public EntityScript ToEntity()
        {
            var expressions = GetEntityExpressions();

            var expressionSet = new EntityScript(
                this.ScriptId,
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
