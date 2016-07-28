using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Entity;
using NetFusion.Domain.Entity.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Domain.Roslyn.Core
{
    public class EntityEvaluationService : IEntityEvaluationService
    {
        // Map between a domain entity and its related set of 
        // dynamic expressions.
        private ILookup<Type, EntityExpressionScript> _entityScripts;

        public void Load(IEnumerable<EntityExpressionSet> expressions)
        {
            var scripts = expressions.SelectMany(es => es.Expressions, (es, e) =>
                new EntityExpressionScript
                {
                    EntityType = es.EntityType,
                    Expression = e,
                    Evaluator = this.CreateScriptRunner(es.EntityType, e.Expression)
                }).ToList();

            _entityScripts = scripts.ToLookup(e => e.EntityType);
        }

        public async Task Evaluate<TEntity>(TEntity entity)
            where TEntity : IAttributedEntity
        {
            var futureResults = new List<Task<object>>();

            var entityScripts = _entityScripts[typeof(TEntity)];
            var entityScope = new EntityScriptScope<TEntity>(entity);

            foreach (EntityExpressionScript script in entityScripts)
            {
                var result = await script.Evaluator(entityScope);
                if (script.Expression.PropertyName != null)
                {
                    entity.SetAttributeValue(script.Expression.PropertyName, result);
                }
            }
        }

        // Used to create a cached delegate that can be used to execute a script
        // against a domain model and a set of dynamic properties.  This is a 
        // compiled delegate.
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
