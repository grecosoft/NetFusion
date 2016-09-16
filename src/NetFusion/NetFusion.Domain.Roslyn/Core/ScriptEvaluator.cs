using NetFusion.Domain.Entity;
using NetFusion.Domain.Scripting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using System;

namespace NetFusion.Domain.Roslyn.Core
{
    /// <summary>
    /// Instance of this class is cached by the EntityScriptingService and contains a set of
    /// expression evaluators that have their expressions compiled to delegates upon first
    /// use.  A script is assigned an unique name within the context of an entity.  If
    /// an entity script is tagged with the name 'default', it is applied first followed by
    /// the script with a specified name.
    /// </summary>
    public class ScriptEvaluator
    {
        public const string DEFAULT_SCRIPT_NAME = "default";
        public EntityScript Script { get; }
        public IEnumerable<ExpressionEvaluator> Evaluators { get; private set; }

        public ScriptEvaluator(EntityScript script)
        {
            Check.NotNull(script, nameof(script));
            this.Script = script;
        }

        public void SetExpressionEvaluators(IEnumerable<ExpressionEvaluator> evaluators)
        {
            Check.NotNull(evaluators, nameof(evaluators));
            this.Evaluators = evaluators;
        }

        public bool IsDefault => this.Script.Name == DEFAULT_SCRIPT_NAME;

        public async Task Execute(object entity)
        {
            Check.NotNull(entity, nameof(entity));

            // The scope that will be used to resolve references made within
            // expressions.  This includes the entity and its set of optional
            // dynamic attributes referenced as _ within scripts..
            Type entityScopeType = typeof(EntityScriptScope<>).MakeGenericType(this.Script.EntityType);
            object entityScope = entityScopeType.CreateInstance(entity);

            foreach (ExpressionEvaluator evaluator in this.Evaluators
                .OrderBy(ev => ev.Expression.Sequence))
            {
                object result = await evaluator.Invoker(entityScope);
                var attributedEntity = entity as IAttributedEntity;

                // Determines if a dynamic calculated attribute and not an assignment to
                // static domain-entity property.  If so update or add the attribute's value.
                if (attributedEntity != null && evaluator.Expression.AttributeName != null)
                {
                    attributedEntity.Attributes.SetValue(evaluator.Expression.AttributeName, result);
                }
            }
        }
    }
}
