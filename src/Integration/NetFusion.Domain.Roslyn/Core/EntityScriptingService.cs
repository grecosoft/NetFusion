using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Domain.Entities;
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
    /// Uses Roslyn to execute a named script against a domain entity.  A script with the name 'default' is executed
    /// before the specified named script if present.  The 'default' script can contain common expressions that are
    /// common among multiple named scripts.
    /// </summary>
    public class EntityScriptingService : IEntityScriptingService
    {
        private readonly ILogger<EntityScriptingService> _logger;

        // Map between a domain entity and its related set of named scripts.
        private ILookup<Type, ScriptEvaluator> _scriptEvaluators;

        public EntityScriptingService(ILoggerFactory logger)
        {
            Check.NotNull(logger, nameof(logger));
            _logger = logger.CreateLogger<EntityScriptingService>();
        }

        // Initialize the script evaluators that should be invoked for a
        // given entity.  The expressions will be compiled upon first use.
        public void Load(IEnumerable<EntityScript> scripts)
        {
            Check.NotNull(scripts, nameof(scripts));

            _scriptEvaluators = scripts.Select(s => new ScriptEvaluator(s))
                .ToLookup(se => se.Script.EntityType);
        }

        public async Task ExecuteAsync(object entity, string scriptName = "default")
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(scriptName, nameof(scriptName));

            Type entityType = entity.GetType();

            IEnumerable<ScriptEvaluator> scripts = _scriptEvaluators[entityType];
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
            if (scriptName != ScriptEvaluator.DEFAULT_SCRIPT_NAME)
            {
                ScriptEvaluator namedEvalScript = scripts.FirstOrDefault(se => se.Script.Name == scriptName);
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
            var preEvalDetails = GetPreEvalDetails(entity, evaluator);

            using (var durationLogger = _logger.LogTraceDuration(ScriptingLogEvents.SCRIPT_EVALUATION, "Script Evaluation"))
            {
                durationLogger.Log.LogTraceDetails(ScriptingLogEvents.SCRIPT_PRE_EVALUATION, 
                    "Pre-Evaluation Details", preEvalDetails);

                CompileScript(evaluator);
                SetDefaultAttributeValues(evaluator.Script, entity);
                await evaluator.ExecuteAsync(entity);

                durationLogger.Log.LogTraceDetails(ScriptingLogEvents.SCRIPT_POST_EVALUATION, 
                    "Post-Evaluation Details", new { PostEvalValues = entity });
            }
        }

        private object GetPreEvalDetails(object entity, ScriptEvaluator evaluator)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                return new
                {
                    PreEvalValues = entity,
                    Script = evaluator.Script.Expressions
                        .OrderBy(e => e.Sequence)
                        .Select(e => new { e.AttributeName, e.Expression })
                };
            }
            return new { };
        }

        private void CompileScript(ScriptEvaluator scriptEval)
        {
            if (scriptEval.Evaluators == null)
            {
                lock (scriptEval)
                {
                    if (scriptEval.Evaluators == null)
                    {
                        ExpressionEvaluator[] evaluators = CreateExpressionEvaluators(scriptEval.Script);
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
        // the attribute from a prior evaluation or manually specified by the caller.
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

        // A script can specify assemblies and name spaces that should be
        // imported for types used by the expressions.
        private IList<Assembly> GetImportedAssemblies(EntityScript script, 
            IEnumerable<Type> assembliesContainingTypes)
        {
            var assemblies = new List<Assembly>();
            foreach (string assemblyName in script.ImportedAssemblies ?? new string[] { })
            {
                assemblies.Add(Assembly.Load(new AssemblyName(assemblyName)));
            }

            var defaultAssemblies = assembliesContainingTypes.Select(t => t.GetTypeInfo().Assembly);
            assemblies.AddRange(defaultAssemblies);

            return assemblies.Distinct().ToList();
        }

        public async Task<bool> SatifiesPredicate(object entity, ScriptPredicate predicate)
        {
            Check.NotNull(entity, nameof(entity));
            Check.NotNull(predicate, nameof(predicate));

            var attributedEntity = entity as IAttributedEntity;
            if (attributedEntity == null)
            {
                throw new InvalidOperationException(
                    $"The entity being evaluated must implement: {typeof(IAttributedEntity)}");
            }

            await this.ExecuteAsync(entity, predicate.ScriptName);

            if (!attributedEntity.Attributes.Contains(predicate.AttributeName))
            {
                throw new InvalidOperationException(
                    $"After the predicate script named: {predicate.ScriptName} was executed, " + 
                    $"the expected predicate attribute named: {predicate.AttributeName} was not" +
                    $"calculated.");
            }

            object value = attributedEntity.Attributes.GetValue(predicate.AttributeName);
            if (value.GetType() != typeof(bool))
            {
                throw new InvalidOperationException(
                    $"After the predicate script named: {predicate.ScriptName} was executed, " +
                    $"the expected predicate attribute named: {predicate.AttributeName} was not" +
                    $"a Boolean value type.  The type of the value was: {value.GetType()}.");
            }

            return (bool)value;
        }
    }
}
