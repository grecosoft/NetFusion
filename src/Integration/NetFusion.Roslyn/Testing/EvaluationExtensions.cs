using Microsoft.Extensions.Logging;
using NetFusion.Base.Entity;
using NetFusion.Base.Scripting;
using NetFusion.Domain.Roslyn.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

namespace NetFusion.Domain.Roslyn.Testing
{
    /// <summary>
    /// Provides extension methods used to test expression evaluations.
    /// </summary>
    public static class EvaluationExtensions
    {
        public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
            string propertyName,
            string expression) 
        {
            expressions.Add(new EntityExpression(expression, 0, propertyName));
            return expressions;
        }

        public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
            string expression) 
        {
            expressions.Add(new EntityExpression(expression, 0));
            return expressions;
        }

        public static IEntityScriptingService CreateService<TEntity>(this IList<EntityExpression> expressions)
            where TEntity : IAttributedEntity
        {
            var es = new EntityScript(
                Guid.NewGuid().ToString(),
                "default",
                typeof(TEntity).AssemblyQualifiedName,
                new ReadOnlyCollection<EntityExpression>(expressions));

            es.ImportedAssemblies = new[] { typeof(Common.Extensions.ObjectExtensions).GetTypeInfo().Assembly.FullName };
            es.ImportedNamespaces = new[] { typeof(Common.Extensions.ObjectExtensions).Namespace };


            var loggerFactory = new LoggerFactory();

            var evalSrv = new EntityScriptingService(loggerFactory);

            evalSrv.Load(new EntityScript[] { es });
            return evalSrv;
        }
    }
}
