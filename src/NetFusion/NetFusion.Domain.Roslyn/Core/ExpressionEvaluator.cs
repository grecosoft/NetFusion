using Microsoft.CodeAnalysis.Scripting;
using NetFusion.Domain.Scripting;

namespace NetFusion.Domain.Roslyn.Core
{
    /// <summary>
    /// The entity expression and the associated compiled
    /// script to be executed at runtime.
    /// </summary>
    public class ExpressionEvaluator
    {
        public ExpressionEvaluator(
            EntityExpression expression,
            ScriptRunner<object> executor)
        {
            this.Expression = expression;
            this.Invoker = executor;
        }

        /// <summary>
        /// The expression from which the evaluator was created.
        /// </summary>
        public EntityExpression Expression { get; }

        /// <summary>
        /// The compiled script to be executed at runtime.
        /// </summary>
        public ScriptRunner<object> Invoker { get; }
    }
}
