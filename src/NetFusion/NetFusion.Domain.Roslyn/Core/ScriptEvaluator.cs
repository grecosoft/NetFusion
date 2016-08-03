using NetFusion.Domain.Entity;
using NetFusion.Domain.Scripting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using NetFusion.Common;

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
        public const string DefaultScriptName = "default";
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

        public bool IsDefault => this.Script.Name == DefaultScriptName;

        public async Task Execute<TEntity>(TEntity entity)
            where TEntity : class
        {
            Check.NotNull(entity, nameof(entity));

            // The scope that will be used to resolve references made within
            // expressions.  This includes the entity and its set of optional
            // dynamic attributes.
            var entityScope = new EntityScriptScope<TEntity>(entity);

            foreach (ExpressionEvaluator evaluator in this.Evaluators
                .OrderBy(ev => ev.Expression.Sequence))
            {
                var result = await evaluator.Invoker(entityScope);
                var attributedEntity = entity as IAttributedEntity;

                if (attributedEntity != null &&evaluator.Expression.AttributeName != null)
                {
                    // If the expression corresponds to a dynamic entity attribute, update
                    // it value with the result of the expression.
                    attributedEntity.Attributes.SetValue(evaluator.Expression.AttributeName, result);
                }
            }
        }
    }
}
