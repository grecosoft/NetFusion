using NetFusion.Base.Scripting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NetFusion.Domain.MongoDB.Scripting
{
    /// <summary>
    /// Data model storing a script consisting of a list of ordered expressions.
    /// </summary>
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

        /// <summary>
        /// Constructor.  Created data-model from script entity.
        /// </summary>
        /// <param name="script">The script domain entity.</param>
        public EntityScriptMeta(EntityScript script)
        {
            ScriptId = script.ScriptId;
            EntityType = script.EntityType.AssemblyQualifiedName;
            Name = script.Name;
            Description = script.Description;

            Attributes = script.InitialAttributes;
            ImportedAssemblies = script.ImportedAssemblies;
            ImportedNamespaces = script.ImportedNamespaces;

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

            Expressions = expressionMetadata;
        }

        /// <summary>
        /// Returns an entity script entity from the stored data-model.
        /// </summary>
        /// <returns>Script domain entity.</returns>
        public EntityScript ToEntity()
        {
            var expressions = GetEntityExpressions();

            var expressionSet = new EntityScript(
                ScriptId,
                Name,
                EntityType,
                new ReadOnlyCollection<EntityExpression>(expressions))
            {
                Description = Description,
                InitialAttributes = Attributes,
                ImportedAssemblies = ImportedAssemblies,
                ImportedNamespaces = ImportedNamespaces
            };
            return expressionSet;
        }

        private IList<EntityExpression> GetEntityExpressions()
        {
            return Expressions.Select(e => new EntityExpression(
                e.Expression,
                e.Sequence,
                e.AttributeName) {
                    Description = e.Description

                }).ToList();
        }
    }
}
