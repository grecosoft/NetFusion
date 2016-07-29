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
        private bool _compiledOnLoad;

        public void Load(IEnumerable<EntityScript> scripts, bool compiledOnLoad = false)
        {
            Check.NotNull(scripts, nameof(scripts));

            _compiledOnLoad = compiledOnLoad;

            _scriptEvaluators = scripts.Select(CreateScriptEvaluator)
                .ToLookup(se => se.Script.EntityType);
        }

        private ScriptEvaluator CreateScriptEvaluator(EntityScript script)
        {
            var scriptEval = new ScriptEvaluator(script);

            if (_compiledOnLoad)
            {
                var evaluators = CreateExpressionEvaluators(script);
                scriptEval.SetExpressionEvaluators(evaluators);
            }
            return scriptEval;
        }

        private ExpressionEvaluator[] CreateExpressionEvaluators(EntityScript script)
        {
            return script.Expressions.Select(exp =>
            {
                var scriptRunner = CreateScriptRunner(script, exp.Expression);
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
            var defaultEvalScript = scripts.FirstOrDefault(se => se.IsDefault);
            
            if (defaultEvalScript != null)
            {
                CompileScript(defaultEvalScript);
                SetDefaultAttributeValues(defaultEvalScript.Script, entity);
                await defaultEvalScript?.Execute(entity);
            }
            
            // Execute the specified script.
            if (scriptName != ScriptEvaluator.DefaultScriptName)
            {
                var namedEvalScript = scripts.FirstOrDefault(se => se.Script.Name == scriptName);
                if (namedEvalScript == null)
                {
                    throw new InvalidOperationException(
                        $"A script named: {scriptName} for the entity type of: {entityType} could not be found.");
                }

                CompileScript(namedEvalScript);
                SetDefaultAttributeValues(namedEvalScript.Script, entity);
                await namedEvalScript.Execute(entity);
            }
        }

        private void SetDefaultAttributeValues(EntityScript script, IAttributedEntity entity)
        {
            foreach (var attribute in script.Attributes)
            {
                if (!entity.ContainsAttribute(attribute.Key))
                {
                    entity.SetAttributeValue(attribute.Key, attribute.Value);
                }
            }
        }

        // Used to create a cached delegate that can be used to execute a script
        // against a domain model and a set of dynamic properties.  
        private ScriptRunner<object> CreateScriptRunner(EntityScript script, string expression)
        {
            var scopeType = typeof(EntityScriptScope<>).MakeGenericType(script.EntityType);
            var options = GetScriptOptions(script);

            var scriptRunner = CSharpScript.Create<object>(expression, options, scopeType);
            return scriptRunner.CreateDelegate();
        }

        // Can be called to compile all scripts that have not yet been compiled.
        public void CompileAllScripts()
        {
            foreach (var scriptEval in _scriptEvaluators.Values())
            {
                CompileScript(scriptEval);
            }
        }

        private void CompileScript(ScriptEvaluator scriptEval)
        {
            if (_compiledOnLoad)
            {
                return;
            }

            if (scriptEval.Evaluators == null)
            {
                lock(scriptEval)
                {
                    if (scriptEval.Evaluators == null)
                    {
                        var evaluators = CreateExpressionEvaluators(scriptEval.Script);
                        scriptEval.SetExpressionEvaluators(evaluators);
                    }
                }
            }
        }

        private ScriptOptions GetScriptOptions(EntityScript script)
        {
            var defaultTypes = new Type[]
            {
                typeof(DynamicObject),
                typeof(CSharpArgumentInfo),
                typeof(ExpandoObject),
                typeof(ObjectExtensions),
                script.EntityType
            };

            var importedAssemblies = GetImportedAssemblies(script, defaultTypes);
            var options = ScriptOptions.Default.AddReferences(importedAssemblies)
                .AddImports(script.ImportedNamespaces);

            return options;
        }

        private IList<Assembly> GetImportedAssemblies(EntityScript script, IEnumerable<Type> assembliesContainingTypes)
        {
            var assemblies = new List<Assembly>();
            foreach (string assemblyName in script.ImportedAssemblies)
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }

            var defaultAssemblies = assembliesContainingTypes.Select(Assembly.GetAssembly);
            assemblies.AddRange(defaultAssemblies);

            return assemblies.Distinct().ToList();
        }
    }
}
