using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetFusion.Common.Base.Entity;
using NetFusion.Common.Base.Scripting;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Services.Roslyn.Internal;

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
    public IEnumerable<ExpressionEvaluator>? Evaluators { get; private set; }

    public ScriptEvaluator(EntityScript script)
    {
        Script = script ?? throw new ArgumentNullException(nameof(script));
    }

    public void SetExpressionEvaluators(IEnumerable<ExpressionEvaluator> evaluators)
    {
        Evaluators = evaluators ?? throw new ArgumentNullException(nameof(evaluators));
    }

    public bool IsDefault => Script.Name == DefaultScriptName;

    public async Task ExecuteAsync(object entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // The scope that will be used to resolve references made within
        // expressions.  This includes the entity and its set of optional
        // dynamic attributes referenced as _ within scripts.
        Type entityScopeType = typeof(EntityScriptScope<>).MakeGenericType(Script.EntityType);
        object entityScope = entityScopeType.CreateInstance(entity);

        if (Evaluators == null)
        {
            return;
        }

        foreach (ExpressionEvaluator evaluator in Evaluators
                     .OrderBy(ev => ev.Expression.Sequence))
        {
            object result = await evaluator.ScriptRunner(entityScope);

            // Determines if a dynamic calculated attribute and not an assignment to
            // static domain-entity property.  If so update or add the attribute value.
            if (entity is IAttributedEntity attributedEntity && evaluator.Expression.AttributeName != null)
            {
                attributedEntity.Attributes.SetValue(evaluator.Expression.AttributeName, result);
            }
        }
    }
}