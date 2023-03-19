using System;
using Microsoft.CodeAnalysis.Scripting;
using NetFusion.Common.Base.Scripting;

namespace NetFusion.Services.Roslyn.Internal;

/// <summary>
/// The entity expression and the associated compiled script to be executed at runtime.
/// </summary>
public class ExpressionEvaluator
{
    public ExpressionEvaluator(
        EntityExpression expression,
        ScriptRunner<object> scriptRunner)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        ScriptRunner = scriptRunner ?? throw new ArgumentNullException(nameof(scriptRunner));
    }

    /// <summary>
    /// The expression from which the ScriptRunner was created.
    /// </summary>
    public EntityExpression Expression { get; }

    /// <summary>
    /// The expression compiled to a delegate to be executed at runtime.
    /// </summary>
    public ScriptRunner<object> ScriptRunner { get; }
}