using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Entity;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Base.Scripting;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Services.Roslyn.Internal;

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

    public EntityScriptingService(ILogger<EntityScriptingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scriptEvaluators = Enumerable.Empty<ScriptEvaluator>().ToLookup(se => se.Script.EntityType);
    }

    // Initialize the script evaluators that should be invoked for a
    // given entity.  The expressions will be compiled upon first use.
    public void Load(IEnumerable<EntityScript> scripts)
    {
        if (scripts == null) throw new ArgumentNullException(nameof(scripts));

        _scriptEvaluators = scripts.Select(s => new ScriptEvaluator(s))
            .ToLookup(se => se.Script.EntityType);
    }

    public async Task ExecuteAsync(object entity, string scriptName = "default")
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(scriptName);

        Type entityType = entity.GetType();

        IEnumerable<ScriptEvaluator> scripts = _scriptEvaluators[entityType].ToArray();
        if (scripts.Empty())
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
            ScriptEvaluator? namedEvalScript = scripts.FirstOrDefault(se => se.Script.Name == scriptName);
            if (namedEvalScript == null)
            {
                throw new InvalidOperationException(
                    $"A script named: {scriptName} for the entity type of: {entityType} could not be found.  " +
                    "Note: Script names are case-sensitive.");
            }

            await ExecuteScript(entity, namedEvalScript);
        }
    }

    private async Task ExecuteScript(object entity, ScriptEvaluator evaluator)
    {
        var preEvalDetails = GetPreEvalDetails(evaluator);

        using (_logger.LogDebugDuration("Script Evaluation"))
        {
            var log = LogMessage.For(LogLevel.Debug, "Pre-Evaluation: {EntityType}", entity.GetType().Name)
                .WithProperties(
                    LogProperty.ForName("EvalDetails", preEvalDetails),
                    LogProperty.ForName("Entity", entity)
                );

            _logger.Log(log);

            CompileScript(evaluator);
            SetDefaultAttributeValues(evaluator.Script, entity);
                
            await evaluator.ExecuteAsync(entity);
                
            _logger.LogDetails(LogLevel.Debug, "Post-Evaluation: {EntityType}", entity, entity.GetType().Name);
        }
    }

    private object GetPreEvalDetails(ScriptEvaluator evaluator)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            return new
            {
                Script = evaluator.Script.Expressions
                    .OrderBy(e => e.Sequence)
                    .Select(e => new { e.AttributeName, e.Expression })
            };
        }
        return new { };
    }

    private static void CompileScript(ScriptEvaluator scriptEval)
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

    private static ExpressionEvaluator[] CreateExpressionEvaluators(EntityScript script)
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
    private static void SetDefaultAttributeValues(EntityScript script, object entity)
    {
        if (entity is not IAttributedEntity attributedEntity)
        {
            return;
        }

        foreach (var (name, value) in script.InitialAttributes)
        {
            attributedEntity.Attributes.SetValue(name, value, overrideIfPresent: false);
        }
    }

    // Used to create a cached delegate that can be used to execute a script
    // against a domain model and a set of dynamic attributes.  
    private static ScriptRunner<object> CreateScriptRunner(EntityScript script, string expression)
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

    private static ScriptOptions GetScriptOptions(EntityScript script)
    {
        var defaultTypes = new[]
        {
            typeof(DynamicObject),
            typeof(CSharpArgumentInfo),
            typeof(ExpandoObject),
            typeof(ObjectExtensions),
            script.EntityType
        };

        var importedAssemblies = GetImportedAssemblies(script, defaultTypes);
            
        var options = ScriptOptions.Default.AddReferences(importedAssemblies)
            .AddImports(script.ImportedNamespaces ?? Array.Empty<string>());

        return options;
    }

    // A script can specify assemblies and name spaces that should be
    // imported for types used by the expressions.
    private static IEnumerable<Assembly> GetImportedAssemblies(EntityScript script, 
        IEnumerable<Type> assembliesContainingTypes)
    {
        var assemblies = new List<Assembly>();

        // Add the assemblies containing the list of specified types.
        var defaultAssemblies = assembliesContainingTypes.Select(t => t.Assembly);
        assemblies.AddRange(defaultAssemblies);

        if (script.ImportedAssemblies == null)
        {
            return assemblies;
        }

        // If specified, load the additional assemblies specified by the script.
        foreach (string assemblyName in script.ImportedAssemblies)
        {
            try
            {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                assemblies.Add(assembly);
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loadErrors = ex.LoaderExceptions.Select(le => le?.Message)
                    .Where(m => m != null)
                    .Distinct()
                    .ToArray();
                    
                throw new ScriptException("Error loading plug-in assembly.", ex, "LoadErrors", loadErrors);
            }
            catch (Exception ex)
            {
                throw new ScriptException(
                    $"Error loading assembly: {assemblyName}",
                    ex);
            }
        }

        // Return the distinct list of assemblies.
        return assemblies.Distinct().ToArray();
    }

    public async Task<bool> SatisfiesPredicateAsync(object entity, ScriptPredicate predicate)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        if (entity is not IAttributedEntity attributedEntity)
        {
            throw new InvalidOperationException(
                $"The entity being evaluated must implement: {typeof(IAttributedEntity)}");
        }

        await ExecuteAsync(entity, predicate.ScriptName);

        if (! attributedEntity.Attributes.Contains(predicate.AttributeName))
        {
            throw new InvalidOperationException(
                $"After the predicate script named: {predicate.ScriptName} was executed, " + 
                $"the expected predicate attribute named: {predicate.AttributeName} was not" +
                "calculated.");
        }

        object? value = attributedEntity.Attributes.GetValue(predicate.AttributeName);
        if (value == null)
        {
            throw new InvalidOperationException("");
        }
            
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