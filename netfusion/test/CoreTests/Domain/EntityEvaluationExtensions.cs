using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommonTests.Base.Entity;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Entity;
using NetFusion.Base.Scripting;
using NetFusion.Roslyn.Core;

namespace CoreTests.Domain
{
    public static class EntityEvaluationTestExtensions
    {
        public static IList<EntityExpression> AddExpression<T>(this IList<EntityExpression> expressions,
            string propertyName,
            string expression) where T: IAttributedEntity
        {
            expressions.Add(new EntityExpression(expression, 0, propertyName));
            return expressions;
        }

        public static IList<EntityExpression> AddExpression<T>(this IList<EntityExpression> expressions,
            string expression) where T : IAttributedEntity
        {
            expressions.Add(new EntityExpression(expression, 0));
            return expressions;
        }

        public static IEntityScriptingService CreateService(this IList<EntityExpression> expressions)
        {
            var es = new EntityScript(
                Guid.NewGuid().ToString(),
                "default", 
                typeof(DynamicEntity).AssemblyQualifiedName,
                new ReadOnlyCollection<EntityExpression>(expressions));

            es.ImportedAssemblies = new[] { typeof(NetFusion.Common.Extensions.ObjectExtensions).Assembly.FullName };
            es.ImportedNamespaces = new[] { typeof(NetFusion.Common.Extensions.ObjectExtensions).Namespace };

            var loggerFactory = new LoggerFactory();
                
            var evalSrv = new EntityScriptingService(loggerFactory);
            evalSrv.Load(new EntityScript[] { es });
            return evalSrv;
        }


    }
}
