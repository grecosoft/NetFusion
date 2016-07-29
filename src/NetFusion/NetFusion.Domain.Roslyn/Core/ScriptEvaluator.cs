using NetFusion.Domain.Entity;
using NetFusion.Domain.Scripting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace NetFusion.Domain.Roslyn.Core
{
    public class ScriptEvaluator
    {
        public const string DefaultScriptName = "default";
        public EntityScript Script { get; }
        public IEnumerable<ExpressionEvaluator> Evaluators { get; private set; }

        public ScriptEvaluator(EntityScript script)
        {
            this.Script = script;
        }

        public void SetExpressionEvaluators(IEnumerable<ExpressionEvaluator> evaluators)
        {
            this.Evaluators = evaluators;
        }

        public bool IsDefault => this.Script.Name == DefaultScriptName;

        public async Task Execute<TEntity>(TEntity entity)
            where TEntity : IAttributedEntity
        {
            var entityScope = new EntityScriptScope<TEntity>(entity);

            foreach (ExpressionEvaluator evaluator in this.Evaluators
                .OrderBy(ev => ev.Expression.Sequence))
            {
                var result = await evaluator.Invoker(entityScope);
                if (evaluator.Expression.PropertyName != null)
                {
                    entity.SetAttributeValue(evaluator.Expression.PropertyName, result);
                }
            }
        }
    }
}
