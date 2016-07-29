using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Entity;
using NetFusion.Domain.Scripting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Domain.Roslyn.Core
{
    /// <summary>
    /// Uses Roslyn to execute a named script against a domain entity.
    /// A script with the name 'default' is executed before the specified
    /// named script if present.  The 'default' script can contain common
    /// expressions that are common among multiple named scripts.
    /// </summary>
    public class EntityScriptingService : IEntityScriptingService
    {
        // Map between a domain entity and its related set of scripts.
        private ILookup<Type, ScriptEvaluator> _scriptEvaluators;

        public void Load(IEnumerable<EntityScript> scripts)
        {
            Check.NotNull(scripts, nameof(scripts));

            _scriptEvaluators = scripts.Select(CreateScriptEvaluator)
                .ToLookup(se => se.Script.EntityType);
        }

        private ScriptEvaluator CreateScriptEvaluator(EntityScript script)
        {
            var evaluators = CreateExpressionEvaluators(script);
            return new ScriptEvaluator(script, evaluators);
        }

        private ExpressionEvaluator[] CreateExpressionEvaluators(EntityScript script)
        {
            return script.Expressions.Select(exp =>
            {
                var scriptRunner = CreateScriptRunner(script.EntityType, exp.Expression);
                return new ExpressionEvaluator(exp, scriptRunner);

            }).ToArray();
        }

        public async Task Execute<TEntity>(TEntity entity, string scriptName = "default")
            where TEntity : class, IAttributedEntity
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(scriptName, nameof(scriptName));

            var entityType = typeof(TEntity);

            var scripts = _scriptEvaluators[entityType];
            if (scripts == null)
            {
                return;
            }

            // Execute the default script if specified.
            var defaultScript = scripts.FirstOrDefault(se => se.IsDefault);
            await defaultScript?.Execute(entity);

            // Execute the specified script.
            if (scriptName != ScriptEvaluator.DefaultScriptName)
            {
                var namedScript = scripts.FirstOrDefault(se => se.Script.Name == scriptName);
                if (namedScript == null)
                {
                    throw new InvalidOperationException(
                        $"A script named: {scriptName} for the entity type of: {entityType} could not be found.");
                }
                await namedScript.Execute(entity);
            }
        }

        // Used to create a cached delegate that can be used to execute a script
        // against a domain model and a set of dynamic properties.  
        private ScriptRunner<object> CreateScriptRunner(Type entityType, string expression)
        {
            var scopeType = typeof(EntityScriptScope<>).MakeGenericType(entityType);
            var options = GetScriptOptions(entityType);

            var script = CSharpScript.Create<object>(expression, options, scopeType);
            return script.CreateDelegate();
        }

        private ScriptOptions GetScriptOptions(Type entityType)
        {
            var options = ScriptOptions.Default.AddReferences(
                Assembly.GetAssembly(typeof(DynamicObject)),
                Assembly.GetAssembly(typeof(CSharpArgumentInfo)),
                Assembly.GetAssembly(typeof(ExpandoObject)),
                Assembly.GetAssembly(typeof(ObjectExtensions)),
                Assembly.GetAssembly(entityType))
                    .AddImports(
                        "System.Dynamic", 
                        "NetFusion.Common.Extensions");
            return options;
        }
    }
}
