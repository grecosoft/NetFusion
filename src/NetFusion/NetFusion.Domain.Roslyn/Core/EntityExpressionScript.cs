using Microsoft.CodeAnalysis.Scripting;
using NetFusion.Domain.Entity;
using System;

namespace NetFusion.Domain.Roslyn.Core
{
    /// <summary>
    /// Object containing an domain entity dynamic script expression
    /// and its compiled version that that will execute with performance
    /// of statically written code.
    /// </summary>
    public class EntityExpressionScript
    {
        public Type EntityType { get; set; }
        public EntityPropertyExpression Expression { get; set; }
        public ScriptRunner<object> Evaluator { get; set; }
    }
}
