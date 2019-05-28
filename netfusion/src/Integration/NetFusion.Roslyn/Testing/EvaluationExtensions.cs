using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Entity;
using NetFusion.Base.Scripting;
using NetFusion.Roslyn.Core;
// ReSharper disable UseObjectOrCollectionInitializer

namespace NetFusion.Roslyn.Testing
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
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            
            expressions.Add(new EntityExpression(expression, 0, propertyName));
            return expressions;
        }

        public static IList<EntityExpression> AddExpression(this IList<EntityExpression> expressions,
            string expression) 
        {
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));
            
            expressions.Add(new EntityExpression(expression, 0));
            return expressions;
        }

        public static IEntityScriptingService CreateService<TEntity>(this IList<EntityExpression> expressions,
            IDictionary<string, object> initialAttributes = null)
            where TEntity : IAttributedEntity
        {
            var es = new EntityScript(
                Guid.NewGuid().ToString(),
                "default",
                typeof(TEntity).AssemblyQualifiedName,
                new ReadOnlyCollection<EntityExpression>(expressions));

            es.ImportedAssemblies = new[] { typeof(Common.Extensions.ObjectExtensions).GetTypeInfo().Assembly.FullName };
            es.ImportedNamespaces = new[] { typeof(Common.Extensions.ObjectExtensions).Namespace };

            if (initialAttributes != null)
            {
                es.InitialAttributes = initialAttributes;
            }
            
            var loggerFactory = new LoggerFactory();

            var evalSrv = new EntityScriptingService(loggerFactory);

            evalSrv.Load(new[] { es });
            return evalSrv;
        }
    }
}
