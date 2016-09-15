using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using NetFusion.Bootstrap.Logging;
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
        private readonly IContainerLogger _logger;

        // Map between a domain entity and its related set of named scripts.
        private ILookup<Type, ScriptEvaluator> _scriptEvaluators;

        public EntityScriptingService(IContainerLogger logger)
        {
            Check.NotNull(logger, nameof(logger));
            _logger = logger.ForContext<EntityScriptingService>();
        }

        // Initialize the script evaluators that should be invoked for a
        // given entity.  The expressions will be compiled upon first use.
        public void Load(IEnumerable<EntityScript> scripts)
        {
            Check.NotNull(scripts, nameof(scripts));

            _scriptEvaluators = scripts.Select(s => new ScriptEvaluator(s))
                .ToLookup(se => se.Script.EntityType);
        }

        public async Task Execute(object entity, string scriptName = "default")
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(scriptName, nameof(scriptName));

            var entityType = entity.GetType();

            var scripts = _scriptEvaluators[entityType];
            if (scripts == null)
            {
                return;
            }

            // Execute the default script if specified.
            var defaultEvalScript = scripts.FirstOrDefault(se => se.IsDefault);

            if (defaultEvalScript != null)
            {
                await ExecuteScript(entity, defaultEvalScript);
            }

            // Execute script with a specified name.
            if (scriptName != ScriptEvaluator.DefaultScriptName)
            {
                var namedEvalScript = scripts.FirstOrDefault(se => se.Script.Name == scriptName);
                if (namedEvalScript == null)
                {
                    throw new InvalidOperationException(
                        $"A script named: {scriptName} for the entity type of: {entityType} could not be found.");
                }

                await ExecuteScript(entity, namedEvalScript);
            }
        }

        private async Task ExecuteScript(object entity, ScriptEvaluator evaluator)
        {
            var preEvalDetails = new
            {
                PreEvalValues = entity,
                Script = evaluator.Script.Expressions
                    .OrderBy(e => e.Sequence)
                    .Select(e => new { e.AttributeName, e.Expression })
            };

            using (var log = _logger.VerboseDuration("Script Evaluation", preEvalDetails))
            {
                CompileScript(evaluator);
                SetDefaultAttributeValues(evaluator.Script, entity);
                await evaluator.Execute(entity);

                log.SetCompletionDetails(new { PostEvalValues = entity });
            }
        }

        private void CompileScript(ScriptEvaluator scriptEval)
        {
            if (scriptEval.Evaluators == null)
            {
                lock (scriptEval)
                {
                    if (scriptEval.Evaluators == null)
                    {
                        var evaluators = CreateExpressionEvaluators(scriptEval.Script);
                        scriptEval.SetExpressionEvaluators(evaluators);
                    }
                }
            }
        }

        private ExpressionEvaluator[] CreateExpressionEvaluators(EntityScript script)
        {
            return script.Expressions.Select(exp =>
            {
                var scriptRunner = CreateScriptRunner(script, exp.Expression);
                return new ExpressionEvaluator(exp, scriptRunner);

            }).ToArray();
        }

        // A script can specify the default values that should be used for an entity's 
        // dynamic attributes.  These are only set if the entity doesn't already have
        // the attribute from a prior evaluation.
        private void SetDefaultAttributeValues(EntityScript script, object entity)
        {
            var attributedEntity = entity as IAttributedEntity;
            if (attributedEntity == null)
            {
                return;
            }

            foreach (var attribute in script.InitialAttributes)
            {
                if (!attributedEntity.Attributes.Contains(attribute.Key))
                {
                    attributedEntity.Attributes.SetValue(attribute.Key, attribute.Value);
                }
            }
        }

        // Used to create a cached delegate that can be used to execute a script
        // against a domain model and a set of dynamic attributes.  
        private ScriptRunner<object> CreateScriptRunner(EntityScript script, string expression)
        {
            var scopeType = typeof(EntityScriptScope<>).MakeGenericType(script.EntityType);
            var options = GetScriptOptions(script);

            var scriptRunner = CSharpScript.Create<object>(expression, options, scopeType);
            return scriptRunner.CreateDelegate();
        }

        // Can be called to compile all scripts that have not yet been compiled.  This can 
        // be used to verify all script expression during development.
        public void CompileAllScripts()
        {
            foreach (var scriptEval in _scriptEvaluators.Values())
            {
                CompileScript(scriptEval);
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
                .AddImports(script.ImportedNamespaces ?? new string[] { });

            return options;
        }

        // A script can specify assemblies and name spaces that should be imported for types used
        // by the expressions.
        private IList<Assembly> GetImportedAssemblies(EntityScript script, IEnumerable<Type> assembliesContainingTypes)
        {
            var assemblies = new List<Assembly>();
            foreach (string assemblyName in script.ImportedAssemblies ?? new string[] { })
            {
                assemblies.Add(Assembly.Load(assemblyName));
            }

            var defaultAssemblies = assembliesContainingTypes.Select(Assembly.GetAssembly);
            assemblies.AddRange(defaultAssemblies);

            return assemblies.Distinct().ToList();
        }

        public async Task<bool> SatifiesPredicate(object entity, ScriptPredicate predicate)
        {
            var attributedEntity = entity as IAttributedEntity;
            if (attributedEntity == null)
            {
                throw new InvalidOperationException(
                    $"The entity being evaluated must implement: {typeof(IAttributedEntity)}");
            }

            await this.Execute(entity, predicate.ScriptName);

            if (!predicate.AttributeName.IsNullOrWhiteSpace())
            {
                return attributedEntity.Attributes.GetValue<bool>(predicate.AttributeName);
            }
            return false;
        }
    }
}
